# IR2IL — Claude Notes

## What this project is

IR2IL is a proof-of-concept LLVM IR to MSIL (CIL) translator. It compiles LLVM IR (`.ll` files produced by clang) into .NET assemblies using `System.Reflection.Emit`, then runs them on the CLR.

## Running tests

The solution file is at `src/IR2IL.sln`. Always run tests from the repo root:

```bash
dotnet test src
```

Filtering tests (MSUnit parameter syntax doesn't work directly — use `Name~` instead):

```bash
dotnet test src --filter "Name~fibonacci"
dotnet test src --filter "Name~Arbitrary"
```

The `Benchmark` tests (the programs in `tests/benchmarks`) only check that the output and exit code match the native binary. Their timings are measured by `src/IR2IL.Benchmarks` (BenchmarkDotNet).

Processes started by the tests (`RunProgram`) are killed after 120 s (override with `IR2IL_TEST_TIMEOUT_SECONDS`), so a miscompiled infinite loop fails the test with a `TimeoutException` instead of hanging the run.

Each test's `.il` and decompiled `.cs` dumps (under `src/IR2IL.Tests/bin/<config>/<tfm>/output/`) are only written when the test fails. Set `IR2IL_DUMP=1` to write them for every test.

Clang output (`.ll` files and native binaries) is cached in `build/cache/clang`, keyed on the clang binary, flags, source and neighbouring headers. Delete that folder to force a rebuild.

List available tests:

```bash
dotnet test src --filter "FullyQualifiedName~Arbitrary" --list-tests
```

## Architecture

### Compilation pipeline

1. **`Compiler`** — entry point, coordinates the pipeline
2. **`ModuleCompiler`** — owns the `LLVMModuleRef`, `TypeBuilder`, and `TypeSystem`; orchestrates compilation of globals and functions
3. **`CompiledModule`** — holds the mapping from LLVM values to compiled .NET members (`MethodInfo`, `FieldInfo`); passed to all IL emitters
4. **`FunctionILEmitter`** — emits MSIL for a single LLVM function body
5. **`GlobalsILEmitter`** — emits the static constructor that initialises global variables
6. **`TypeSystem`** — maps LLVM types to .NET types; owns struct type definitions

### Key classes

- `src/IR2IL/ModuleCompiler.cs` — compiles the LLVM module; creates `CompiledModule` (passing `_typeBuilder` so it can generate methods at compile time)
- `src/IR2IL/CompiledModule.cs` — function/global lookup + on-demand method generation (e.g. `GetOrCreatePrintfOverload`)
- `src/IR2IL/ILEmission/FunctionILEmitter.cs` — main instruction emitter
- `src/IR2IL/Intrinsics/` — LLVM intrinsic handlers (`IntrinsicFunction` subclasses, registered in `IntrinsicFunctions.LLVMIntrinsics`)
- `src/IR2IL.Runtime/` — small runtime library linked into compiled programs

### Adding LLVM intrinsics

New intrinsics are registered in `src/IR2IL/Intrinsics/IntrinsicFunctions.cs`. Use `StandardIntrinsicFunction`:

- **Scalar BCL methods** (unambiguous): `StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.Sqrt))`
- **Scalar BCL methods** (ambiguous overloads, e.g. `MathF.Log`): pass explicit parameter types — `StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.Log), typeof(float))`
- **Vector BCL methods with concrete typed overloads** (e.g. `Vector128.Exp`, `Vector64.Log`): use `Create` with the concrete vector parameter type: `StandardIntrinsicFunction.Create(typeof(Vector128), nameof(Vector128.Exp), typeof(Vector128<double>))`
- **Generic-only vector methods** (e.g. `Vector128.Sqrt<T>`, `Vector128.Sum<T>`): use `StandardIntrinsicFunction.CreateGeneric` with the element type as the type argument: `StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sqrt), typeof(double))`. To tell which kind a method is, check via reflection — `Create` fails at startup with `Method X not found` if you use it for a generic-only method.
- **Custom helpers**: add a method to `src/IR2IL.Runtime/VectorUtility.cs` (or `LLVMIntrinsics.cs`) and register with `StandardIntrinsicFunction.Create(typeof(VectorUtility), nameof(VectorUtility.MyMethod))`

### Arbitrary-width integer types

LLVM IR allows integers of any bit width (e.g. `i40` from a struct with 3+29+3-bit fields that clang packs into 40 bits). .NET only has 8/16/32/64-bit integers.

**Representation**: `TypeSystem.GetIntegerType` rounds any non-standard width up to the next container type via `RoundUpToTypeSize` — so `i40` → `long`, `i24` → `int`, `i9` → `short`, `i72`/`i80`/`i128` → `Int128`. The container is the *storage* type only: see "Normalisation" below for what the bits above the logical width contain.

**Normalisation**: an `iN` is only defined in its low N bits. Whatever produced it — a `load` (sign-extended, because `i8`/`i16` map to `sbyte`/`short`), a `trunc`, an `add` — may have left anything above them on the IL stack, and producers don't clean up. Instead, **every consumer that depends on the upper bits extends its operands itself**, through one function:

- `EmitIntegerOperand(value, signedness)` pushes an operand and calls `EmitNormalizedInteger(bits, signedness)`: `Signed` sign-extends from bit N-1 (`conv.i1`/`conv.i2`, or `shl` then `shr` by the unused bits), `Unsigned` clears the bits above N-1 (`conv.u1`/`conv.u2`, or an `and` mask). It does nothing for 32, 64 or 128 bits. For 65–127 bits, where the value is an `Int128`, `Signed` shifts left then arithmetic-right by `128 - N`, and `Unsigned` ands with `EmitWideIntegerMask`.
- `Signedness.Any` is for operations whose low N bits don't depend on the operands' upper bits (`add`, `sub`, `mul`, `and`, `or`, `xor`, the value operand of `shl`), so they skip it.
- The consumers are `icmp` (signed or unsigned by predicate; `eq`/`ne` unsigned), `br`/`select` conditions (an `i1` is true when its low bit is set), `switch` (zero-extended condition and cases), `zext`/`sext`, `sitofp`/`uitofp`, `inttoptr`, `sdiv`/`srem`/`udiv`/`urem`, `lshr`/`ashr` and every shift amount, GEP indices (signed), and vector element indices.
- Conversions (`EmitIntegerConversion`) only change the stack type (`conv.i4`/`u4`/`i8`/`u8`); `trunc` doesn't mask.
- When adding an instruction or intrinsic that reads an integer operand, push it with `EmitIntegerOperand` unless it truly ignores the upper bits. `tests/generators/generate_narrow_integer_tests.py` generates tests that cross every way of producing a narrow integer with the consumers.
- Constants are producers like any other: up to 64 bits, `EmitConstantIntegerValue` pushes the sign-extended value (`i24 -1` is `-1`), so a constant operand needs the same normalisation. Wider constants are `Int128` parsed from the printed (signed) value (`EmitWideConstantIntegerValue`). Nothing masks a constant before a store: stores write exactly ceil(N/8) bytes.

**Integers wider than 64 bits** (i65–i128): stored as `Int128`, under the same normalisation rules as narrower integers (the bits above N are undefined). Binary operations on these types are handled by `EmitWideIntegerBinaryOp`, which calls `Int128` operator methods directly rather than using IL opcodes. `lshr` routes through `WideIntegerHelper.LshrInt128` (casts to `UInt128` for the unsigned shift). Wide integer shift amounts (e.g. `lshr i72 %x, i72 64`) are emitted directly as `Ldc_I4` for constants — never pushed as `Int128`, since `Conv_I4` doesn't work on a struct. Loading wide integers from memory uses `WideIntegerHelper.LoadWideInt`. Narrowing from `Int128` to ≤64-bit targets goes through `WideIntegerHelper.Int128ToLong` first; `zext`/`sext`/`trunc` between two wide types leave the `Int128` as it is.

**Struct sizing**: LLVM computes `sizeof({ i40 })` as 8 bytes on ARM64 (5 bytes + 3 padding for 8-byte alignment), matching a .NET struct with a single `long` field.

**Loads and stores** move exactly ceil(N/8) bytes (`EmitStoreIntegerIndirect`, `EmitLoadPartialInteger`; wide loads use `LoadWideInt`): a non-power-of-2 width that is narrower than its container (`i24`, `i40`, `i72`) goes through a local and `cpblk`, so neighbouring bytes are untouched and nothing past the object is read. A partial load zero-fills the rest of the container.

### Small vectors (16 and 32 bits)

.NET has no vector smaller than `Vector64`, so `<2 x i8>`, `<4 x i8>` and `<2 x i16>` (and the `<N x i1>` masks of 2 and 4 lanes, whose lanes are bytes) are held in a `Vector64<T>` with padding lanes (`TypeSystem.GetVectorContainerSizeInBits`). `<3 x float>` in a `Vector128<float>` is the same situation. The padding lanes follow the rule of the bits above an `iN`: they may contain anything, and whatever reads all the lanes ignores them.

- **Loads and stores** move exactly the vector's own bytes (`EmitLoadPartial`, `EmitStorePartial`), as for `i24`; `TypeSystem.IsPaddedVector` says when.
- **Struct fields and array elements** of these vectors are integers of the vector's size (`TypeSystem.GetMsilMemoryType`), so the layout is LLVM's, and `extractvalue`/constant structs go through a partial load/store of the field.
- **`bitcast`** from or to a padded vector goes through a local, as `Unsafe.BitCast` needs equal sizes.
- **Creating a vector from lane values** (`select` on a mask) has to pass `GetVectorContainerLaneCount` arguments: zeros after the real lanes.
- **Shuffles** that join two vectors with `Create(v0, v1)` only apply when the sources have no padding; otherwise the software path is used.
- **Mask resizing** (`TryResizeVectorMask`) uses `Vector64.Narrow(m, m)` and `Vector64.WidenLower(m)` for masks of 64 bits or less.
- `Vector1024` is a custom type of two `Vector512`s, with few operations.

### Handling varargs on macOS ARM64

CoreCLR on macOS ARM64 does **not** support the managed vararg calling convention (`CallingConventions.VarArgs`). P/Invoke of native vararg functions (e.g. `printf`) also does not work reliably on this platform.

**Solution for known functions**: intercept calls to known vararg C functions in `FunctionILEmitter.EmitCall` and route them to generated managed implementations instead.

**Tests using varargs** that cannot be handled this way are skipped on non-Windows by adding them to the ignored list (the exact list depends on which test suite it is) in `CompilerTests.cs`.

For `printf` specifically:
- `FunctionILEmitter.EmitCall` detects `functionToCall.Name == "printf"` when `isVarArg` is true
- It calls `CompiledModule.GetOrCreatePrintfOverload(allParamTypes)` to get (or lazily generate) a concrete typed wrapper method
- The generated `__printf(void* fmt, T arg, ...)` method boxes its arguments into an `object[]` and calls `PrintfHelper.PrintfCore(IntPtr, object[])` in `IR2IL.Runtime`
- `PrintfHelper.PrintfCore` parses the C format string and writes to `Console.Write`

The typed `__printf` overloads are generated as `MethodBuilder` instances during compilation (before `TypeBuilder.CreateType()` is called), so they live in the emitted assembly rather than the runtime library.
