# IR2IL — Claude Notes

## What this project is

IR2IL is a proof-of-concept LLVM IR to MSIL (CIL) translator. It compiles LLVM IR (`.ll` files produced by clang) into .NET assemblies using `System.Reflection.Emit`, then runs them on the CLR.

## Running quick throwaway C# programs

`dotnet script` and `dotnet-script` are **not available** in this environment. To run a one-off C# snippet (e.g. to inspect BCL APIs via reflection), create a temp project manually:

```bash
mkdir -p /tmp/probe && cd /tmp/probe && cat > probe.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
EOF
cat > Program.cs << 'EOF'
using System;
using System.Linq;
using System.Reflection;
// ... your code here
EOF
dotnet run
```

Include all necessary `using` directives — implicit global usings are not always available for all APIs (e.g. `System.Reflection` must be explicit).

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

**Representation**: `TypeSystem.GetIntegerType` rounds any non-standard width up to the next container type via `RoundUpToTypeSize` — so `i40` → `long`, `i24` → `int`, `i9` → `short`, `i72`/`i80`/`i128` → `Int128`. The upper bits of the container are kept clear.

**Arithmetic masking**: `EmitUnaryOrBinaryOperation` emits a bitwise AND after each scalar opcode when the result type is a non-power-of-2 width. This keeps bits above the logical width zeroed so that subsequent operations (comparisons, sign extension) see correct values. For example, `add i40` computes in `long` then masks with `0xFFFFFFFFFF`.

**Integers wider than 64 bits** (i65–i128): stored as `Int128`. Binary operations on these types are handled by `EmitWideIntegerBinaryOp`, which calls `Int128` operator methods directly rather than using IL opcodes. `lshr` routes through `WideIntegerHelper.LshrInt128` (casts to `UInt128` for the unsigned shift). Wide integer shift amounts (e.g. `lshr i72 %x, i72 64`) are emitted directly as `Ldc_I4` for constants — never pushed as `Int128`, since `Conv_I4` doesn't work on a struct. Loading wide integers from memory uses `WideIntegerHelper.LoadWideInt`. Narrowing from `Int128` to ≤64-bit targets goes through `WideIntegerHelper.Int128ToLong` first.

**Struct sizing**: LLVM computes `sizeof({ i40 })` as 8 bytes on ARM64 (5 bytes + 3 padding for 8-byte alignment), matching a .NET struct with a single `long` field.

**Not yet implemented**: `sext` from a non-power-of-2 source (e.g. `sext i40 to i64`) — the sign bit would need shifting to be correctly extended. Also `stind` for wide integer types.

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
