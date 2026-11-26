# Replacing `VectorMath`: direct calls to .NET vector APIs

Status: done (2026-11-26). Covers two items of [TODO.md](TODO.md) §3: "Resolve intrinsics by pattern instead of exact name" (for the math, integer and reduction intrinsics) and part of "Audit the intrinsics and vector code for `System.Runtime.Intrinsics` coverage".

This plan is written to be executed step by step without further design work. Every API it names was checked against .NET 11 (SDK 11.0.100) on ARM64 by reflection, and every codegen claim was measured with `DOTNET_JitDisasm`. If something here turns out to be wrong, stop and record it rather than improvising a different design.

## 1. Why

`src/IR2IL.Runtime/VectorMath.cs` is a small generic framework: operation structs (`Sin<T>`, `IntegerMax<T>`, `Add<T>`, …) implementing static-abstract interfaces, and generic lane loops `Unary/Binary/Ternary<TVector, TLane, TOperation>(…, laneCount)` and `Reduce<…>`. The three intrinsic resolvers emit calls to it.

Problems:

1. **The name is wrong.** It serves scalar integer intrinsics and reductions too, and it collides with the BCL's internal `System.Runtime.Intrinsics.VectorMath`, which appears in JIT disassembly next to it.
2. **It hides direct BCL calls.** The emitted IL calls `VectorMath.Binary<Vector128<short>, short, IntegerMax<short>>(x, y, 8)` where `Vector128.Max(x, y)` exists.
3. **The JIT doesn't clean it up.** Measured on ARM64, fully optimised:

| Case | Through `VectorMath` | Direct | What happens |
|---|---|---|---|
| `smax <4 x i32>` | 120 bytes, every lane through the stack | 20 bytes, one `smax v.4s` | The loop unrolls, but `Unsafe.As<TVector, TLane>(ref x)` exposes the vector's address, so nothing stays in registers |
| `sin <8 x float>` | Loop of 8 `float.Sin` calls through memory | 2 × `Vector128.Sin` | Not unrolled |
| `reduce.add <4 x i32>` | A call to `Reduce`, a runtime `laneCount` check, a dead scalar loop, then a call to `ReduceVector128` (`ext/add/tbl/add`) | One `addv` | `Reduce` isn't inlined |

The dead branches *do* fold once everything is inlined (`Unsafe.SizeOf<T>()`, `typeof(T) == typeof(…)`, `IsSupported` and constant arguments are all folded). So helpers with ISA branches marked `AggressiveInlining`, as in `VectorUtility`, are sound. The cost comes from reinterpreting a generic `TVector` through a reference, so **new helpers take concrete `Vector64<T>`…`Vector512<T>` parameters and never use `Unsafe.As` on a reference.**

## 2. Rules of the new design

In order of preference, each intrinsic is emitted as:

1. **A direct call to the cross-platform BCL method** on the container type (`Vector64/128/256/512.<Op>`, concrete or generic), or `Math`/`MathF`/`int`/`uint`/… for scalars.
2. **A call to a helper of ours with ISA branches** (`AdvSimd`, `Sse41`, …) and a fallback, marked `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, only where no cross-platform API exists but target-specific instructions do. These are: vector `ctpop` (`VectorUtility.PopCount`) and the `mul/and/or/xor/smax/smin/umax/umin` reductions (new `VectorReduction` class).
3. **A scalar fallback emitted directly as IL**: an unrolled `GetElement` → `MathF.Tan` → `WithElement` sequence per real lane. There's no runtime lane loop any more.

Decided:

- **BCL vector transcendentals are accepted** although they aren't bit-identical to libm (user decision, 2026-11-25). Measured: `Vector64/128/256/512.Sin`, `Cos`, `Asin`, `Exp` and `Log`, for `float` and `double`, agree with `MathF`/`Math` on every value of the `math_intrinsics.ll` table under its mantissa mask (low 13 bits of a `float` and 28 bits of a `double` cleared). `sin(1e30)`, denormals, infinities and NaN all pass.
- **`log10` uses the scalar fallback**, not the existing `Log2(x) * (1/log2(10))` composition in `VectorUtility`. It's a custom composition, not a BCL method, and it rounds twice.
- **Padding lanes** (`<3 x i32>` in a `Vector128<int>`, `<2 x i8>` in a `Vector64<sbyte>`) don't matter for element-wise operations. Reductions overwrite them with the operation's identity first; see §4.4.
- **`Vector1024`** gets the methods it needs, with the BCL names, so the same name lookup works on it (§4.1). Each one applies the `Vector512` method to both halves.

## 3. What each intrinsic becomes

`N` stands for the container's non-generic type (`Vector64`, `Vector128`, `Vector256`, `Vector512`, `Vector1024`), from `TypeSystem.GetNonGenericVectorType`. `T` is the lane type from `TypeSystem.GetMsilVectorElementType`, which is signed for integers. `U` is its unsigned counterpart.

### 3.1 Floating point math: `llvm.<op>.f32|f64|v<n>f32|v<n>f64`

| Op | Arity | Scalar `float` / `double` | Vector |
|---|---|---|---|
| `sin` | 1 | `MathF.Sin` / `Math.Sin` | `N.Sin` (concrete overloads) |
| `cos` | 1 | `Cos` | `N.Cos` (concrete) |
| `asin` | 1 | `Asin` | `N.Asin` (concrete) |
| `exp` | 1 | `Exp` | `N.Exp` (concrete) |
| `log` | 1 | `Log` (one-parameter overload) | `N.Log` (concrete) |
| `sqrt` | 1 | `Sqrt` | `N.Sqrt<T>` (generic) |
| `fabs` | 1 | `Abs` | `N.Abs<T>` (generic) |
| `ceil` | 1 | `Ceiling` | `N.Ceiling` (concrete) |
| `copysign` | 2 | `CopySign` | `N.CopySign<T>` (generic) |
| `fmuladd` | 3 | `FusedMultiplyAdd` | `N.FusedMultiplyAdd` (concrete) |
| `tan`, `acos`, `atan`, `sinh`, `cosh`, `tanh`, `log10` | 1 | `Tan`, `Acos`, … | Scalar fallback per lane |
| `atan2`, `pow` | 2 | `Atan2`, `Pow` | Scalar fallback per lane |

Scalars always use `MathF` for `float` and `Math` for `double`, resolved with explicit parameter types (`typeof(MathF).GetMethodStrict("Log", [typeof(float)])`). The scalar fallback for vectors calls the same methods.

The current `math_intrinsics.ll` tests vectors of 2, 3, 4, 8 and 16 lanes, so `Vector1024<double>` (`<16 x double>`) and padded `<3 x float>` are covered.

### 3.2 Integer intrinsics: `abs`, `smin`, `smax`, `umin`, `umax`, `ctpop`, `usub.sat`, `scmp`, `ucmp`

Scalars of up to 64 bits keep today's operand handling. Operands go through `EmitSignedInteger` or `EmitUnsignedInteger`, widened to `int`/`uint` for 32 bits or less and to `long`/`ulong` above. Only the method called changes.

| Op | Scalar method (32-bit / 64-bit) | Vector |
|---|---|---|
| `abs` | `LLVMIntrinsics.Abs(int)` / `Abs(long)` (new, wrapping) | `N.Abs<T>` |
| `smin` / `smax` | `Math.Min(int, int)` / `Math.Min(long, long)`, and `Max` likewise | `N.Min<T>` / `N.Max<T>` |
| `umin` / `umax` | `Math.Min(uint, uint)` / `Math.Min(ulong, ulong)`, and `Max` likewise | `As<T, U>` on each operand, `N.Min<U>` / `N.Max<U>`, then `As<U, T>` |
| `ctpop` | `uint.PopCount(uint)` / `ulong.PopCount(ulong)` | `VectorUtility.PopCount<T>` (new, §5.1) |
| `usub.sat` | `LLVMIntrinsics.UnsignedSubtractSaturate(uint, uint)` / `(ulong, ulong)` (new) | `As<T, U>`, `N.SubtractSaturate<U>`, `As<U, T>` |
| `scmp` / `ucmp` | `LLVMIntrinsics.Compare<T>` (moved from `VectorMath.Compare`), plus `conv.i8` when the result is wider than 32 bits, as today | No vector form (unchanged) |

Checked semantics:

- `int.Abs(int.MinValue)` throws `OverflowException`, hence the new wrapping `LLVMIntrinsics.Abs`.
- `Vector128.Abs` wraps: `Abs(int.MinValue) == int.MinValue`, which is LLVM's `abs` with the poison flag clear.
- `Vector128.SubtractSaturate` on unsigned lanes gives 0 when `y > x`.
- `Vector128.Max` on `ulong` lanes is an unsigned max.

`N.As<TFrom, TTo>` is the two-type-argument reinterpret method, present on `Vector64` through `Vector512`. Use it rather than `AsUInt32` and friends, so one code path serves every lane width.

### 3.3 Reductions: `llvm.vector.reduce.<op>.<shape>`

Integer lanes must be 8, 16, 32 or 64 bits, as today; `i1` vectors stay unsupported. Floats are `float` or `double`.

| Op | Lane type | Identity (for padding) | Call |
|---|---|---|---|
| `add` | `T` | 0 | `N.Sum<T>` (`Vector1024.Sum` exists) |
| `mul` | `T` | 1 | `VectorReduction.Multiply<T>` |
| `and` | `T` | all ones (-1) | `VectorReduction.And<T>` |
| `or` | `T` | 0 | `VectorReduction.Or<T>` |
| `xor` | `T` | 0 | `VectorReduction.Xor<T>` |
| `smax` | `T` | `T.MinValue` | `VectorReduction.Max<T>` |
| `smin` | `T` | `T.MaxValue` | `VectorReduction.Min<T>` |
| `umax` | `U` (reinterpret with `As<T, U>` first) | 0 | `VectorReduction.Max<U>` |
| `umin` | `U` | `U.MaxValue` | `VectorReduction.Min<U>` |
| `fadd` with `reassoc` | `T` | `-0.0` | `start + N.Sum<T>(v)` |
| `fmul` with `reassoc` | `T` | `1.0` | `start * VectorReduction.Multiply<T>(v)` |
| `fadd` / `fmul` without `reassoc` | `T` | none | Ordered chain emitted as IL (§4.5) |

An unsigned result (a `byte` or `ushort` lane) is left on the stack as is. It's an `iN` whose upper bits are undefined, so consumers normalise it as usual.

`fmax/fmin/fmaximum/fminimum` reductions stay unimplemented; see §8.

## 4. Compiler side

### 4.1 Shared helpers: new file `src/IR2IL/Intrinsics/VectorIntrinsicEmitter.cs`

`internal static class VectorIntrinsicEmitter`, used by the three resolvers.

```csharp
/// The BCL (or Vector1024) method `name` of `nonGenericVectorType` that takes `arity` vectors of type `vectorType`:
/// a concrete overload (Vector128.Sin(Vector128<float>)) if there is one, else the generic definition
/// (Vector128.Max<T>(Vector128<T>, Vector128<T>)) made for `laneType`. Throws when neither exists.
public static MethodInfo GetVectorMethod(Type nonGenericVectorType, string name, Type vectorType, Type laneType, int arity)
{
    var concrete = nonGenericVectorType.GetMethod(name, BindingFlags.Public | BindingFlags.Static, Enumerable.Repeat(vectorType, arity).ToArray());
    if (concrete != null) return concrete;
    var definition = vectorType.GetGenericTypeDefinition();
    var generic = nonGenericVectorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .SingleOrDefault(m => m.Name == name && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1
            && m.GetParameters().Length == arity
            && m.GetParameters().All(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == definition));
    return generic?.MakeGenericMethod(laneType)
        ?? throw new InvalidOperationException($"{nonGenericVectorType.Name}.{name} not found for {vectorType}.");
}

/// Emits `N.As<TFrom, TTo>` on the vector at the top of the stack.
public static void EmitReinterpret(ILGenerator il, Type nonGenericVectorType, Type fromLane, Type toLane);

/// Emits a constant of `laneType` (ldc.i4 + conv for 8/16-bit, ldc.i4, ldc.i8, ldc.r4, ldc.r8).
public static void EmitLaneConstant(ILGenerator il, Type laneType, object value);

/// Replaces the padding lanes of the vector at the top of the stack (lanes laneCount and above) with `identity`:
///   ConditionalSelect(LessThan(Vector<T>.Indices, Create((T)laneCount)), v, Create((T)identity))
/// Uses a local for v (the mask has to be pushed first). Throws NotImplementedException for Vector1024.
public static void EmitFillPaddingLanes(ILGenerator il, Type nonGenericVectorType, Type vectorType, Type laneType, int laneCount, object identity);

/// The scalar fallback: stores the `arity` vector operands at the top of the stack in locals, then for each real
/// lane i emits   [r] ldc.i4 i; (ldloc a_k; ldc.i4 i; call N.GetElement<T>) × arity; call scalar; call N.WithElement<T>
/// starting with r = a_0, so the padding lanes keep the values of the first operand.
public static void EmitPerLane(ILGenerator il, Type nonGenericVectorType, Type vectorType, Type laneType, int laneCount, int arity, MethodInfo scalar);
```

Notes for these helpers:

- `Indices` is the static property `Vector128<T>.Indices`; it exists on `Vector64<T>` through `Vector512<T>`. Get it with `vectorType.GetProperty("Indices")!.GetMethod`. The scalar `Create` is `nonGenericVectorType.GetMethodStrict("Create", [laneType])`, and `LessThan` and `ConditionalSelect` are generic: use `GetVectorMethod` with arity 2 and 3. All the lane-count and identity values are constants, and the JIT folds the whole mask to a constant load. Measured for `<3 x i32>` sum: `ldr q16, [const]; movi; bsl; addv`.
- `GetElement` and `WithElement` are generic extension methods on `Vector64` through `Vector512` and also on `Vector1024`: `GetStaticMethodStrict(nameof(Vector128.GetElement)).MakeGenericMethod(laneType)`, as `FunctionILEmitter` already does at line ~715.
- Locals come from `il.DeclareLocal(vectorType)`.

**Lane-type helpers.** Move `GetLaneBits`, `IsFloatingPointLane`, `GetIntegerLaneType`, `GetUnsignedLaneType`, `GetWiderLaneType` and `GetNarrowerLaneType` from `FunctionILEmitter.VectorConversions.cs` (lines ~245–300) into `internal static class LaneTypes` in `src/IR2IL/Helpers/LaneTypes.cs`, and make them `public`. Update their callers in `FunctionILEmitter.VectorConversions.cs` and `FunctionILEmitter.cs:1829`. The resolvers use `LaneTypes.GetUnsignedLaneType`, which replaces the two `ToUnsigned` copies.

### 4.2 `LLVMFloatMathIntrinsicFunction`: rename and rewrite `LLVMVectorMathIntrinsicFunction`

File `src/IR2IL/Intrinsics/LLVMFloatMathIntrinsicFunction.cs` (delete the old file). It handles scalars and vectors, so every `llvm.<op>.f32|f64` entry can leave the table.

```csharp
// Intrinsic base name -> (.NET name of the scalar and vector methods, arity, whether N has a vector method).
private static readonly Dictionary<string, (string Name, int Arity, bool HasVectorMethod)> Operations = new()
{
    ["sin"] = ("Sin", 1, true), ["cos"] = ("Cos", 1, true), ["asin"] = ("Asin", 1, true),
    ["exp"] = ("Exp", 1, true), ["log"] = ("Log", 1, true), ["sqrt"] = ("Sqrt", 1, true),
    ["fabs"] = ("Abs", 1, true), ["ceil"] = ("Ceiling", 1, true), ["copysign"] = ("CopySign", 2, true),
    ["fmuladd"] = ("FusedMultiplyAdd", 3, true),
    ["tan"] = ("Tan", 1, false), ["acos"] = ("Acos", 1, false), ["atan"] = ("Atan", 1, false),
    ["sinh"] = ("Sinh", 1, false), ["cosh"] = ("Cosh", 1, false), ["tanh"] = ("Tanh", 1, false),
    ["log10"] = ("Log10", 1, false), ["atan2"] = ("Atan2", 2, false), ["pow"] = ("Pow", 2, false),
};
```

- `TryCreate(name, resultType, typeSystem, out intrinsic)` accepts `llvm.<op>.<suffix>` (3 dot-separated parts) whose result type is `float`, `double`, or a vector of them.
- The scalar method is `(isFloat ? typeof(MathF) : typeof(Math)).GetMethodStrict(Name, Enumerable.Repeat(laneType, Arity).ToArray())`.
- `BuildCall`:
  - **Scalar:** emit the `Arity` operands, then `call scalar`.
  - **Vector with a vector method:** emit the operands, then `call GetVectorMethod(N, Name, vectorType, laneType, Arity)`. Resolve it in `TryCreate`, so a missing method fails at compile time, not silently.
  - **Vector without one:** emit the operands, then `VectorIntrinsicEmitter.EmitPerLane(…, laneCount: (int)resultType.VectorSize, …, scalar)`.
- `IsPure => true`.

### 4.3 `LLVMIntegerIntrinsicFunction`: rewrite in place

Keep the class name, the `FindBaseName` logic (base names with a dot such as `usub.sat`), the scalar operand handling and the `conv.i8` for `scmp`/`ucmp`. Replace the operation table with:

```csharp
// Intrinsic base name -> (arity, read as signed, vector method name or null for VectorUtility.PopCount / no vector form).
["abs"] = (1, true, "Abs"), ["smin"] = (2, true, "Min"), ["smax"] = (2, true, "Max"),
["umin"] = (2, false, "Min"), ["umax"] = (2, false, "Max"), ["usub.sat"] = (2, false, "SubtractSaturate"),
["ctpop"] = (1, false, null), ["scmp"] = (2, true, null), ["ucmp"] = (2, false, null),
```

Scalar methods, by `(op, 64-bit?)`, see §3.2:

| Op | Method |
|---|---|
| `abs` | `LLVMIntrinsics.Abs` overload |
| `smin` / `smax` | `Math.Min` / `Math.Max` with `int` or `long` parameters |
| `umin` / `umax` | `Math.Min` / `Math.Max` with `uint` or `ulong` parameters |
| `ctpop` | `typeof(uint)` or `typeof(ulong)` `.GetMethodStrict("PopCount", [type])` |
| `usub.sat` | `LLVMIntrinsics.UnsignedSubtractSaturate` overload |
| `scmp` / `ucmp` | `LLVMIntrinsics.Compare<T>` |

Vector `BuildCall` (lanes of 8 to 64 bits only, as today; `scmp`/`ucmp` return false for vectors):

- **Signed (`abs`, `smin`, `smax`):** emit the operands, then `call GetVectorMethod(N, name, vectorType, T, arity)`.
- **Unsigned (`umin`, `umax`, `usub.sat`):** emit each operand followed by `EmitReinterpret(T → U)`, then `call GetVectorMethod(N, name, vectorOfU, U, arity)`, then `EmitReinterpret(U → T)`.
- **`ctpop`:** emit the operand, then `call VectorUtility.PopCount<T>`. Pick the overload whose parameter is `vectorType`, by name and parameter type definition. `VectorUtility` has other methods that aren't `PopCount`, so filter on the name.

Remember that `abs` has a second, `i1 immarg`, operand that isn't pushed (only `arity` operands are emitted, as today).

### 4.4 `LLVMVectorReduceIntrinsicFunction`: rewrite in place

Keep `TryCreate`'s signature (the call site in `FunctionILEmitter.EmitCall` passes the `reassoc` flag). In `BuildCall`:

1. For `fadd`/`fmul`, emit the start value (operand 0) first.
2. Emit the vector (operand `^2`).
3. For `umax`/`umin`, `EmitReinterpret(T → U)`.
4. If `TypeSystem.IsPaddedVector(vectorType)` and the reduction isn't ordered, `EmitFillPaddingLanes(…, laneCount: vectorType.VectorSize, identity)` with the identity from §3.3, written as a value of the lane type in use (`U` for the unsigned ones).
5. Call the method from §3.3. For `add` and `fadd`, that's `GetVectorMethod(N, "Sum", …, arity 1)`. For the others, it's the `VectorReduction` overload whose parameter type definition matches the container, made generic for the lane type.
6. For `fadd`/`fmul` with `reassoc`, emit `add` or `mul`, so the start value is the left operand.
7. Ordered `fadd`/`fmul`: see §4.5.

### 4.5 Ordered `fadd`/`fmul`

Push the start value, store the vector in a local, then for each real lane `i`: `ldloc v; ldc.i4 i; call N.GetElement<T>; add` (or `mul`). This combines in order from the start value, which LLVM requires without `reassoc`.

### 4.6 Remove from the exact-name table: `src/IR2IL/Intrinsics/IntrinsicFunctions.cs`

Delete every entry the resolvers now cover:

- **Floating point math:** all `llvm.{acos,asin,atan,atan2,ceil,copysign,cos,cosh,exp,fabs,fmuladd,log,log10,pow,sin,sinh,sqrt,tan,tanh}.*` entries, both scalar and vector.
- **Integer:** `llvm.ctpop.i8`, `llvm.ctpop.i16`, `llvm.scmp.i32.i32`, `llvm.smin.i32`, `llvm.smax.i32`, `llvm.smax.i64` and `llvm.smax.v4i32`.
- **Reductions:** all `llvm.vector.reduce.add.*` entries, including `v12i32` and `v16i64`.

Keep `is.fpclass`, the partial reduce, `cttz.elts`, and the irregular and no-op entries. In `FunctionILEmitter.EmitCall` (line ~2216), replace `LLVMVectorMathIntrinsicFunction.TryCreate` with `LLVMFloatMathIntrinsicFunction.TryCreate`, keeping the same position.

## 5. Runtime side

### 5.1 `VectorUtility.PopCount`: new methods in `src/IR2IL.Runtime/VectorUtility.cs`

Overloads `PopCount<T>(Vector64<T>)`, `PopCount<T>(Vector128<T>)`, `PopCount<T>(Vector256<T>)`, `PopCount<T>(Vector512<T>)` and `PopCount<T>(Vector1024<T>)`, all `AggressiveInlining`. `T` may be signed or unsigned; only the lane width matters.

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Vector128<T> PopCount<T>(Vector128<T> vector)
{
    if (AdvSimd.IsSupported)
    {
        // CNT counts the bits of each byte; UADDLP adds neighbouring lanes into lanes of twice the width.
        var bytes = AdvSimd.PopCount(vector.AsByte());
        if (Unsafe.SizeOf<T>() == 1) return bytes.As<byte, T>();
        var shorts = AdvSimd.AddPairwiseWidening(bytes);
        if (Unsafe.SizeOf<T>() == 2) return shorts.As<ushort, T>();
        var ints = AdvSimd.AddPairwiseWidening(shorts);
        if (Unsafe.SizeOf<T>() == 4) return ints.As<uint, T>();
        return AdvSimd.AddPairwiseWidening(ints).As<ulong, T>();
    }

    // No x86 vector popcount is exposed in .NET 11, so count with SIMD bit tricks, which work everywhere.
    var x = vector.AsByte();
    x -= (x >>> 1) & Vector128.Create((byte)0x55);
    x = (x & Vector128.Create((byte)0x33)) + ((x >>> 2) & Vector128.Create((byte)0x33));
    x = (x + (x >>> 4)) & Vector128.Create((byte)0x0F);
    if (Unsafe.SizeOf<T>() == 1) return x.As<byte, T>();
    var s = x.AsUInt16(); s = (s & Vector128.Create((ushort)0xFF)) + (s >>> 8);
    if (Unsafe.SizeOf<T>() == 2) return s.As<ushort, T>();
    var w = s.AsUInt32(); w = (w & Vector128.Create(0xFFFFu)) + (w >>> 16);
    if (Unsafe.SizeOf<T>() == 4) return w.As<uint, T>();
    var q = w.AsUInt64(); q = (q & Vector128.Create(0xFFFFFFFFul)) + (q >>> 32);
    return q.As<ulong, T>();
}
```

- **`Vector64`:** the same code. `AdvSimd.PopCount(Vector64<byte>)` and `AddPairwiseWidening` for `Vector64<byte>` and `Vector64<ushort>` exist. For 8-byte lanes (a one-lane `Vector64<long>`), use `AdvSimd.AddPairwiseWideningScalar(Vector64<uint>)`, which exists.
- **`Vector256`, `Vector512`, `Vector1024`:** `Create(PopCount(v.GetLower()), PopCount(v.GetUpper()))`.
- **Measured** for `Vector128<uint>` on ARM64: `cnt; uaddlp; uaddlp`, three instructions.

### 5.2 `VectorReduction`: new file `src/IR2IL.Runtime/VectorReduction.cs`

`public static class VectorReduction`, with `Max`, `Min`, `Multiply`, `And`, `Or` and `Xor`. Each has overloads for `Vector64<T>`, `Vector128<T>`, `Vector256<T>`, `Vector512<T>` and `Vector1024<T>`, returns `T`, and is `AggressiveInlining`. Signedness comes from `T` (the compiler passes `uint` lanes for `umax`). Rules:

- **`Vector256` / `Vector512`:** halve with the cross-platform element-wise operation, then call the narrower overload: `Max(Vector128.Max(v.GetLower(), v.GetUpper()))`. **`Vector1024`** does the same with `Vector512.<Op>`.
- **`Vector128` and `Vector64`** carry the ISA branches, then the cross-platform fallback.

ISA branches (only for `Max` and `Min`; ARM64 and x86 have no across-lanes `mul/and/or/xor`):

| Container | Lane type | ARM64 (`AdvSimd.Arm64.IsSupported`) | x86 (`Sse41.IsSupported`) |
|---|---|---|---|
| `Vector128` | 8, 16, 32-bit | `AdvSimd.Arm64.MaxAcross(v).ToScalar()` / `MinAcross` | 16-bit only, see below |
| `Vector128` | 64-bit | Fallback | Fallback |
| `Vector64` | 8, 16-bit | `AdvSimd.Arm64.MaxAcross(v).ToScalar()` / `MinAcross` | — |
| `Vector64` | 32-bit (2 lanes) | `AdvSimd.MaxPairwise(v, v).ToScalar()` / `MinPairwise` | — |
| `Vector64` | 64-bit (1 lane) | `v.ToScalar()` | `v.ToScalar()` |

The x86 16-bit trick uses `PHMINPOSUW` (`Sse41.MinHorizontal`, which puts the unsigned minimum in lane 0), as clang does:

| Reduction | Expression (`u` is `v.AsUInt16()`) |
|---|---|
| `umin` (`ushort`) | `MinHorizontal(u).ToScalar()` |
| `umax` (`ushort`) | `(ushort)~MinHorizontal(~u).ToScalar()` |
| `smin` (`short`) | `(short)(MinHorizontal(u ^ 0x8000).ToScalar() ^ 0x8000)` |
| `smax` (`short`) | `(short)(MinHorizontal(u ^ 0x7FFF).ToScalar() ^ 0x7FFF)` |

Write the typed branches with the `VectorUtility` idiom:

```csharp
if (typeof(T) == typeof(int)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsInt32()).ToScalar();
```

The JIT removes the boxing for value types.

**Fallback (cross-platform).** Combine the vector with itself rotated by half of what is left, then take lane 0. That's today's `VectorMath.ReduceVector128`/`ReduceVector64` shuffle sequence, moved here and made private: rotations of 8, 4, 2 and 1 bytes with `Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)8, 9, …))`, stopping once the rotation is smaller than a lane. To share it across the six operations, keep a **private** `IOperation<T>` with `static abstract Vector64<T> Apply(Vector64<T>, Vector64<T>)` and `Vector128<T> Apply(Vector128<T>, Vector128<T>)`, plus six private structs. It's an implementation detail; emitted IL only calls the public, concrete-per-width methods. For the 64-bit-lane `Vector128` case of `Max`/`Min`, the fallback's single 8-byte rotation does the job.

Don't use `Unsafe.As` on references anywhere in this class; use `AsXxx()`, `As<TFrom, TTo>()` and `GetLower()`/`GetUpper()`.

### 5.3 `Vector1024`: new methods in `src/IR2IL.Runtime/Vector1024.cs`

Add BCL-named methods that apply the `Vector512` method to both halves, so `GetVectorMethod` finds them:

- **Concrete overloads for `float` and `double`** (`Vector1024<float>` and `Vector1024<double>`): `Sin`, `Cos`, `Asin`, `Exp`, `Log`, `Ceiling` and `FusedMultiplyAdd`. These mirror the BCL, where they are concrete.
- **Generic over `T`:** `Sqrt`, `Abs`, `CopySign`, `Min`, `Max`, `SubtractSaturate` and `As<TFrom, TTo>`. For `As`, use `Unsafe.BitCast<Vector1024<TFrom>, Vector1024<TTo>>`.

Example: `public static Vector1024<float> Sin(Vector1024<float> x) => Create(Vector512.Sin(x.GetLower()), Vector512.Sin(x.GetUpper()));`, with `AggressiveInlining`. Generic constraints follow the existing methods (`where T : unmanaged`, plus whatever the `Vector512` method needs). Padded `Vector1024` vectors stay unsupported (`EmitFillPaddingLanes` throws).

### 5.4 Scalar helpers in `src/IR2IL.Runtime/LLVMIntrinsics.cs`

```csharp
// llvm.abs with the poison flag clear: the absolute value of the minimum is the minimum (int.Abs would throw).
public static int Abs(int x) => x < 0 ? -x : x;
public static long Abs(long x) => x < 0 ? -x : x;
// llvm.usub.sat: x - y, or zero when y is larger.
public static uint UnsignedSubtractSaturate(uint x, uint y) => x > y ? x - y : 0;
public static ulong UnsignedSubtractSaturate(ulong x, ulong y) => x > y ? x - y : 0;
// llvm.scmp and llvm.ucmp: -1, 0 or 1, as an int for the caller to widen.
public static int Compare<T>(T x, T y) where T : IBinaryInteger<T> => x > y ? 1 : x < y ? -1 : 0;
```

All are `AggressiveInlining`. The project doesn't enable `checked` arithmetic, so negation wraps.

### 5.5 Deletions

- `src/IR2IL.Runtime/VectorMath.cs`: the whole file.
- `VectorUtility`: the `Log10*`, `Pow*`, `Acos*`, `Asin*`, `Atan*`, `Tan*`, `Sinh*`, `Cosh*`, `Tanh*` and `Atan2*` methods (and their comment headers), and `Scmp`.
- `LLVMIntrinsics.VectorReduceAddV12I32`.
- `src/IR2IL/Intrinsics/LLVMVectorMathIntrinsicFunction.cs`: replaced by §4.2.

Leave `VectorUtility.Remainder`, `VariableShift*` and `ConvertInt64ToSingle` alone; see §8.

## 6. Steps

Each step builds and passes its tests on its own, which makes it a natural commit boundary. Run tests from the repo root.

- [x] **Step 1: shared helpers, no behaviour change.**
  - Add `LaneTypes` (§4.1) and switch `FunctionILEmitter` to it.
  - Add `VectorIntrinsicEmitter` (§4.1) and the `Vector1024` methods (§5.3).
  - Run `dotnet build src` and `dotnet test src --filter "Name~vector"`.
- [x] **Step 2: floating point math.**
  - Add `LLVMFloatMathIntrinsicFunction` (§4.2) and delete `LLVMVectorMathIntrinsicFunction`.
  - Update the `EmitCall` call site.
  - Delete the math table entries (§4.6) and the `VectorUtility` math helpers (§5.5).
  - Run `dotnet test src --filter "Name~math_intrinsics"`, then `--filter "Name~vector_padded_lanes"`.
- [x] **Step 3: integer intrinsics.**
  - Add `VectorUtility.PopCount` (§5.1) and the `LLVMIntrinsics` scalar helpers (§5.4).
  - Rewrite `LLVMIntegerIntrinsicFunction` (§4.3).
  - Delete the integer table entries and `VectorUtility.Scmp`.
  - Cover `Vector1024` integer lanes: in `tests/generators/generate_integer_intrinsic_tests.py` line ~123, change the limit from `bits * lanes > 512` to `> 1024`, which adds `<16 x i64>`. Regenerate with `python3 tests/generators/generate_integer_intrinsic_tests.py` and check that the `.ll` diff only adds functions. If the new shapes fail for a reason outside the intrinsics (loads or `extractelement` of `Vector1024<long>`), revert the generator change and record it here.
  - Run `dotnet test src --filter "Name~integer_intrinsics"`.
- [x] **Step 4: reductions.**
  - Add `VectorReduction` (§5.2) and rewrite `LLVMVectorReduceIntrinsicFunction` (§4.4, §4.5).
  - Delete the `reduce.add` table entries and `VectorReduceAddV12I32`.
  - Run `dotnet test src --filter "Name~vector_reductions"`.
- [x] **Step 5: clean-up and full verification.**
  - Delete `VectorMath.cs`. `grep -rn "VectorMath" src docs CLAUDE.md` must find only this plan and the history lines of `TODO.md`.
  - Update the docs (§7).
  - Run the whole suite, `dotnet test src` (about 6–7 minutes). It must stay all-passing, including the Fujitsu tests: the cached clang IR has 211 `reduce.add`, 66 `smax`, 42 `reduce.fadd`, 6 `reduce.mul` and 4 `reduce.smax` calls.
  - Check the codegen (§6.1).

### 6.1 Checking the codegen

Two checks:

1. **The emitted IL calls BCL methods directly.** Run `IR2IL_DUMP=1 dotnet test src --filter "Name~integer_intrinsics"`, then look in `src/IR2IL.Tests/bin/Debug/net11.0/output/hand-written/integer_intrinsics/` for `Vector128.Max`, `Vector128.SubtractSaturate`, `VectorUtility.PopCount`, and no `VectorMath`.
2. **The helpers compile to the expected instructions.** Use a throwaway console project outside the repo (for example `/tmp/vmprobe`) that references `src/IR2IL.Runtime/IR2IL.Runtime.csproj`. Give it `[MethodImpl(MethodImplOptions.NoInlining)]` wrappers, call each about 200 times from `Main`, and run:
   ```bash
   DOTNET_TieredCompilation=0 DOTNET_JitStdOutFile=/tmp/vmprobe/disasm.txt \
   DOTNET_JitDisasm="<wrapper names>" dotnet bin/Release/net11.0/vmprobe.dll
   ```
   Expected on ARM64:

| Wrapper body | Expected |
|---|---|
| `VectorReduction.Max(Vector128<int>)` | `smaxv` and nothing else of substance, no call |
| `VectorReduction.Max(Vector64<int>)` | `smaxp` |
| `VectorReduction.Min(Vector128<uint>)` | `uminv` |
| `VectorReduction.Multiply(Vector128<int>)` | `ext`/`mul`/`tbl` (or `ext`)/`mul`, no stack traffic, no call |
| `VectorUtility.PopCount(Vector128<int>)` | `cnt`, `uaddlp`, `uaddlp` |
| `<3 x i32>` sum after padding fill (`ConditionalSelect(LessThan(Indices, Create(3)), v, Create(0))` then `Vector128.Sum`) | `ldr q, [const]`, `bsl`, `addv` |

   If a helper shows a `blr` to itself or `str q`/`ldr q` stack traffic, it wasn't inlined or something is reinterpreted through a reference: fix that before finishing.

The x86 branches (`Sse41.MinHorizontal`, the `PopCount` bit-trick fallback) aren't run on ARM64. CI runs the suite on `windows-latest` (x64) as well as `macos-latest` ([.github/workflows/ci.yml](../.github/workflows/ci.yml)), which is where they get tested.

## 7. Documentation updates

- **`CLAUDE.md`, "Adding LLVM intrinsics".** Add that the floating point math (`LLVMFloatMathIntrinsicFunction`), integer (`LLVMIntegerIntrinsicFunction`) and reduction (`LLVMVectorReduceIntrinsicFunction`) intrinsics are resolved by name for every shape, before falling back to the table. A new operation of those families is a row in the resolver's table, not a table entry per shape. State the preference order of §2 (BCL method, then an `AggressiveInlining` helper with ISA branches, then the per-lane IL fallback), and that helpers take concrete `Vector64<T>`…`Vector512<T>` parameters.
- **`CLAUDE.md`, "Small vectors".** Add one bullet: reductions overwrite padding lanes with the identity (`EmitFillPaddingLanes`).
- **`docs/TODO.md`:**
  - **"Resolve intrinsics by pattern" (§3, ~line 144):** rewrite it to say the math, integer and reduction families are done (link this plan), and list what still has exact-name entries (memory intrinsics, `expect`, `objectsize`, …) and the remaining per-width helpers in `VectorUtility` (`IntToPtr`, `MaskFromBits`).
  - **Audit item (~line 165):** remove the `VectorMath` known cases, which this work fixes. Keep the method, and note the x86 paths as CI-tested only.
  - **The done items at ~lines 67 and 69:** they describe `VectorMath` as the current implementation. Edit those sentences to name the new classes.
  - **Mark this work done** in the relevant item.
- **This file:** set "Status" to done, or delete the file if the TODO entries now say everything that matters.

## 8. Out of scope (record, don't do)

- **`VectorUtility.Remainder`, `VariableShift*` and `ConvertInt64ToSingle`** use the same generic `TVector` + `Unsafe.As` lane-loop pattern, so their fallbacks likely have the same stack-spilling codegen. They aren't part of `VectorMath`; worth a separate look with the same method.
- **`fmax`/`fmin`/`fmaximum`/`fminimum` reductions** (unimplemented, TODO §3): `AdvSimd.Arm64.MaxNumberAcross`/`MinNumberAcross` (only `Vector128<float>`) match `maxnum`/`minnum`. They'd fit `VectorReduction` with the same pattern.
- **Vector `ctlz`/`cttz`** (unsupported): `AdvSimd.LeadingZeroCount` (8-, 16- and 32-bit lanes) and `Avx512CD.VL.LeadingZeroCount` (32- and 64-bit lanes) would make it a `VectorUtility` helper like `PopCount`.
- **`i1`-lane reductions** (`reduce.or.v4i1`) and integer lanes that aren't 8, 16, 32 or 64 bits remain unsupported, as today.
