# IR2IL — Recommended next steps

A review of the repository as of 2026-11-19 (commit `d9d18914`). Items are grouped by theme and roughly ordered by priority within each group. File references are relative to the repo root.

Legend: **[bug]** = confirmed or near-certain wrong behaviour, **[gap]** = unsupported feature, **[perf]** = performance, **[cleanup]** = maintainability.

### Current state (measured 2026-11-19, macOS ARM64, Debug, built with `-p:LangVersion=latest`)

| Suite | Tests | Summed test time | Avg / test |
|---|---:|---:|---:|
| FujitsuCompilerTestSuite | 4568 | 2650 s | 0.58 s |
| CTestSuite | 414 | 210 s | 0.51 s |
| Benchmark | 22 | 73 s | 3.3 s |
| Arbitrary | 18 | 51 s | 2.8 s |
| **Total** | **5022, all passing** | | wall clock **6 min 19 s** (user 423 s, **sys 456 s**) |

System CPU time is higher than user time, which suggests process creation and file I/O dominate (three or four process launches plus IL/C# dumps per test), not the compiler itself. See §1.

---

## 0. Urgent

- [x] **[bug] The build is broken on current .NET 10 SDKs.** `src/IR2IL/IR2IL.csproj` sets `<LangVersion>preview</LangVersion>`. With SDK 10.0.402 (allowed by `global.json`'s `rollForward: latestMinor`), the preview "unsafe evolution" rules apply and the build fails with 9 × `CS9363` (pointer conversion operators such as `LLVMValueRef(LLVMOpaqueValue*)` must be used in an unsafe context) in `LLVMExtensions.cs` and `FunctionILEmitter.cs`. Building with `-p:LangVersion=latest` works. Fix: drop `preview` (nothing seems to need it), or add `unsafe` to the affected members. CI will break as soon as its runner image picks up the newer SDK.
- [x] **[bug] The compiler copies `IR2IL.Runtime.dll` from the current directory** (`Compiler.Save`: `File.Copy("IR2IL.Runtime.dll", …)`). That only works when the process's working directory is the compiler's output folder (true for the tests, false for `IR2IL in.ll out.exe` run from anywhere else). Use `typeof(PrintfHelper).Assembly.Location`.

## 1. Test suite

### Speed
Each test case currently:
1. spawns `clang` to produce `.ll`
2. compiles it
3. disassembles the output with ICSharpCode.Decompiler to `.il`
4. **decompiles the whole module to C#**
5. spawns a fresh `dotnet` process to run it

`Arbitrary` and `Benchmark` also spawn `clang` a second time to build and run a native binary.

- [x] **Make the `.il`/`.cs` dumps opt-in** (`IR2IL_DUMP=1`; also written automatically when a test fails). Whole-module C# decompilation is probably the single most expensive step and is only needed when debugging a failure. Could also be done automatically *on failure*.
- [x] **Cache clang output.** Key `.ll` (and native binaries) on (source hash, opt level, clang version) under `build/cache`. The clang toolchain is pinned, so this is safe and saves two process launches per test on re-runs.
- [ ] **Avoid one `dotnet` process start per test.** Run compiled assemblies in a small pool of long-lived worker processes, with the assembly loaded into a collectible `AssemblyLoadContext` and stdout/stderr redirected. Fall back to a fresh process for tests that call `exit`/`abort`.
- [ ] **Move `Benchmark` out of the default run** (`[TestCategory("Benchmark")]`, filtered out in CI). The timings are meaningless when run in parallel with 5,000 other tests, and the tests also compile with clang twice.
- [ ] **CI runs the whole suite twice** (Debug and Release) on both OSes. Consider Release-only on PRs, or splitting the suites across matrix jobs.
- [ ] **Add a timeout to `RunProgram`** and kill the process tree on expiry. Today a miscompiled infinite loop hangs the whole run.
- [ ] Consider moving to MSTest 4 / Microsoft.Testing.Platform (`MSTest` is pinned at 3.6.2) for faster discovery and better parallelism control.

### Coverage and visibility
- [ ] **Add small hand-written `.ll` tests per instruction/intrinsic** (with expected output) that run without clang. They'd be fast and precise, and could cover the bugs listed above: ULE, unsigned vector compares, negative const GEPs, `i40` signed compares, `fcmp` NaN cases, and load/store reordering.
- [ ] **Expand the Fujitsu suite beyond directory `0020`.** Only about 2.6k of its 21k C files are used (`<= 0020` filter). Add directories one at a time and track the pass rate per directory.
- [ ] **Make excluded tests visible.** Move the large `switch` ignore lists in `CompilerTests.cs` into a data file (test id, platform, reason). Report excluded tests as `Inconclusive`/skipped rather than silently dropping them, so tests that start passing get noticed and the reasons stay reviewable.
- [ ] **Re-check stale exclusions.** Several ignore comments mention MSVC-specific problems but apply on all platforms (e.g. `0005_0005`, `0005_0006`, `0014_*` bitfields, `0016_*`/`0018_*` `_Complex`), so they could run on macOS. Similarly, `0021_*` (`extern int printf(...)`) is excluded everywhere but the comment is Windows-specific.
- [ ] Run tests at `-O1`/`-O2`/`-Os` as well as `-O0`/`-O3` once the suite is faster; each level exercises different IR shapes.

## 2. Correctness

### Instruction scheduling / stack inlining
- [ ] **[bug] Side-effect analysis can move loads past stores.** `LLVMExtensions.HasNoSideEffects` returns `true` for `load` and for *every* intrinsic call (including `memcpy`, `memset`, `va_start`). `CanPushToStack` only checks for intervening side effects when the *inlined instruction itself* is a load. So in
  ```llvm
  %v = load i32, ptr %p
  %a = add i32 %v, 1      ; single use, further down
  store i32 0, ptr %p
  call void @use(i32 %a)
  ```
  `%a` (and so `%v`) is emitted at the call, *after* the store. Fix: treat `load` as "reads memory" and refuse to defer any expression tree containing a load past an instruction that may write memory; mark only pure intrinsics (math, `dbg.*`, `assume`, `lifetime.*`) as side-effect free. Add a regression `.ll` test.
- [ ] **[perf/bug] `EmitSwitch` re-emits the condition once per case cluster.** If the condition was inlined (`CanPushToStack`), its whole expression is recomputed for each cluster. Spill it to a local once.

### Integer semantics
- [ ] **[bug] Signed operations on non-power-of-two widths.** Values like `i40` are stored zero-extended in a `long` container, so `icmp slt/sgt/sle/sge`, `sdiv`, `srem`, `ashr` and `sext` see a positive number when the logical sign bit is set. Sign-extend from bit *N-1* (`shl` by `64-N`, then `shr`) before signed operations. (CLAUDE.md already lists `sext`; the others have the same root cause.)
- [ ] **[bug] Loads and stores of non-power-of-two widths access too many bytes.** `EmitLoadIndirect`/`EmitStoreIndirect` round up to the container (`i24` → `ldind.i4`/`stind.i4`, `i40` → 8 bytes). Stores overwrite the bytes after the object, and loads can read past the end of an allocation and don't mask the extra bits. Load and store exactly `ceil(N/8)` bytes (byte-wise or with a narrower load plus a shift), then mask.
- [ ] **[bug] Scalar `icmp ule` (outside a branch) throws `NotImplementedException`.** `EmitICmp` handles UGE/UGT/ULT but not ULE. `EmitBranchCondition` handles it, so this only bites when the result is stored or used by a `select`/`zext`.
- [ ] **[bug] Unsigned vector comparisons use signed element types.** `GetMsilVectorElementType` maps integers to signed types (`sbyte`, `short`, …), and `UGT`/`ULT` call `Vector128.GreaterThan`/`LessThan` on them, which is wrong when the top bit is set. Reinterpret as the unsigned type (`AsByte`, `AsUInt32`, …) first. Vector `SLE`, `UGE`, `ULE` are missing entirely.
- [ ] **[bug] Negative constant GEP offsets are zero-extended.** `ILEmitter.EmitConstantValue` (const-expr GEP) emits `ldc.i4 <offset>; conv.u`; `conv.u` zero-extends an `int32`, so `-8` becomes `0xFFFFFFF8`. Use `conv.i` (or `ldc.i8`). `GetElementPtrConst` also returns `int`, so offsets ≥ 2 GiB overflow.
- [ ] **[bug] `EmitIndexedPtr` throws for variable GEP indices that are not `i64`.** `i32`/`i16` indices are legal and appear in hand-written or non-clang IR. Sign-extend to `native int` instead.
- [ ] `sdiv`/`srem` with `INT_MIN / -1` and division by zero throw .NET exceptions. That's UB in C, so acceptable, but document it.

### Floating point
- [ ] **[bug] Missing or incorrect `fcmp` predicates.** Scalar `EmitFCmp`: `ord`, `ugt`, `true`, `false`. Branch-fused path (`EmitBranchCondition`): also `one`, `ord`, `ugt`. Vector: only OEQ/OGT/OLT/UGE/UNE exist, and vector `UGE` uses `GreaterThanOrEqual`, which is *ordered* (wrong for NaN). The trailing `// TODO: Do the right thing for ordered / unordered equality comparisons` in `EmitFCmp` is still open. Build a table-driven implementation covering all 16 predicates for scalars and vectors, with NaN tests.
- [ ] **[bug] `undef`/`poison` of `double` and `i128` types throw** in `EmitConstantValue` (only `float` and integers up to 64 bits are handled).
- [ ] **[gap]** `half`, `bfloat`, `fp128`, `x86_fp80` types (`0022_*` Fujitsu tests are excluded for `half`).

### Calls, function pointers and the C runtime
- [ ] **[bug/design] Function pointers mix managed and unmanaged calling conventions.** Function addresses are produced with `ldftn` (managed entry points) but indirect calls use `calli unmanaged cdecl`. Calling managed code through an unmanaged `calli` skips the GC transition and is undefined. Passing a callback to native code (`qsort`, `bsearch`, `atexit`, `signal`) needs a real native entry point. Options: emit `[UnmanagedCallersOnly]` thunks for address-taken functions and always call through unmanaged `calli`, or use managed `calli` and only create thunks when a pointer escapes to native code.
- [ ] **[bug] `fprintf` to a stream other than stderr writes to stdout** (`PrintfHelper.FprintfCore`). A `FILE*` from `fopen` should go through native `fwrite`/`fputs` with the formatted bytes.
- [ ] **[bug] Output ordering between managed and native writes.** `printf`/`puts` go through .NET Console streams, while any other libc output (`fputs`, `fwrite`, `write`, `perror`, which are P/Invoked) goes through libc's own `FILE` buffer, so the two can come out interleaved wrongly. Native `exit()` also doesn't flush .NET-side buffers. Pick one sink for all stdio: route everything through libc, or reimplement more of stdio in managed code.
- [ ] **[gap]** `sprintf`, `snprintf`, `vprintf`/`vfprintf`/`vsnprintf`, `scanf` family. `__sprintf_chk` ignores `buflen`.
- [ ] **[gap] `argc`/`argv` are not passed to `main`** (`EntryPointILEmitter` pushes zeros). The TODO already sketches the code.
- [ ] **[bug] `llvm.stacksave`/`stackrestore` are no-ops** while dynamic `alloca` uses `localloc`. A VLA inside a loop grows the stack every iteration and can overflow. Possible fix: give each such scope a heap arena, or allocate VLAs with `NativeMemory` and free them at `stackrestore`.
- [ ] **[design] Host-OS checks instead of the target triple.** Library names, `\x01_` name stripping, calling conventions and so on are chosen with `OperatingSystem.IsWindows()` on the *compiler's* machine. Read the module's target triple / data layout instead, so IR produced for another platform is handled (or rejected) explicitly.
- [ ] **[gap] Hard-coded native libraries.** All external functions and globals are assumed to live in `libc`/`ucrtbase.dll` (`omp_*` → `libomp`/`vcomp140` is flagged as a hack). Add a configurable symbol → library map (CLI option or response file).

### Type system
- [ ] **[bug] Array type names can collide.** `TypeSystem.CreateArrayType` names types `Array_{ElementType.Name}_{length}`, but the cache key is the LLVM type. `[4 x i40]` and `[4 x i64]` both become `Array_Int64_4` (likewise `i24`/`i32`, `i9`/`i16`). Expect a duplicate-type-name failure or an ambiguous assembly. Key the cache on the *.NET* element type, or add a unique suffix.
- [ ] **[bug] Struct layout isn't checked against LLVM's data layout.** The size check in `CreateStructType` is commented out. Prefer `ExplicitLayout` with `FieldOffset` taken from `LLVMOffsetOfElement` and an explicit total size. That covers packed structs, `i40` padding, `Int128` alignment differences and Windows/macOS differences in one place. At minimum, assert that the sizes match.
- [ ] `AnonymousStructIndex` is a mutable `static`, shared across concurrent compilations in the test host. Make it per-`TypeSystem` so output is deterministic.
- [ ] **[gap]** Opaque structs (`IsOpaqueStruct` throws). Scalable vectors, target extension types and non-zero address spaces are also unsupported.

## 3. IR coverage (instructions, constants, intrinsics)

### Instructions not yet implemented
- [ ] `cmpxchg`, `fence`, `atomicrmw` other than integer `add` (`xchg`, `sub`, `and`, `or`, `xor`, `max`/`min`/`umax`/`umin`, `fadd`) → `Interlocked.*`/`Volatile.*`
- [ ] `load atomic`/`store atomic` orderings, and `volatile` loads/stores (`volatile.` prefix). Alignment below the natural alignment should emit the `unaligned.` prefix.
- [ ] `va_arg` (and a portable `va_start`: the current one relies on CoreCLR's `arglist` layout and doesn't work on macOS ARM64)
- [ ] `addrspacecast`, `indirectbr` / `blockaddress`, `callbr`
- [ ] C++ exceptions: `invoke`, `landingpad`, `resume` (map to `try`/`catch` regions with a runtime exception type carrying the C++ exception pointer)
- [ ] `extractvalue`/`insertvalue` with multiple indices; `insertvalue` on **structs** (currently arrays only)
- [ ] `switch` on conditions wider than 32 bits
- [ ] General `shufflevector` lowering via `Vector128.Shuffle` for single-source in-register shuffles (currently anything that isn't a splat or a concatenation goes through memory element by element)

### Constant expressions
- [ ] Only `getelementptr`, `inttoptr` and `ptrtoint` are supported. Add `sub`/`add` (pointer differences between globals, common in tables), `trunc`, `bitcast`, `select` and `icmp`, ideally by calling LLVM to fold them first.

### Intrinsics
- [ ] **[cleanup] Resolve intrinsics by pattern instead of exact name.** `IntrinsicFunctions.LLVMIntrinsics` lists each overload by its mangled name (`llvm.sin.v2f32`, `llvm.sin.v4f32`, …), and `VectorUtility` has 108 hand-written per-width helpers. Parse the name into base name + overload types and dispatch to a generic implementation (`Vector128.Sin<T>`, or element-wise fallback through `Vector*<T>` generics). That would remove most of the table and most per-width helpers.
- [ ] Missing commonly emitted intrinsics (clang `-O3` emits these often):
  - [ ] Bit operations: `ctlz`, `cttz`, `ctpop.i32/i64`, `bswap`, `bitreverse`, `fshl`/`fshr` (rotates) → `BitOperations`/`BinaryPrimitives`
  - [ ] Overflow and saturation: `{s,u}{add,sub,mul}.with.overflow`, `{s,u}{add,sub}.sat`, `{s,u}shl.sat`
  - [ ] Min/max for all widths and vectors: `smin`/`smax`/`umin`/`umax`
  - [ ] Rounding: `floor`, `trunc`, `round`, `roundeven`, `rint`, `nearbyint`, `ceil.f32`, `lround`/`llround`/`lrint`
  - [ ] Other math: `minnum`/`maxnum`/`minimum`/`maximum`, `exp2`, `log2`, `ldexp`, `frexp`, `fma`
  - [ ] Vector reductions: the rest of `vector.reduce.*` (`and`, `or`, `xor`, `smin`, `umin`, `umax`, `fmax`, `fmin`, `fmul`)
  - [ ] Masked memory: `masked.load`/`masked.store`/`masked.gather`/`masked.scatter`
  - [ ] Misc: `trap`/`debugtrap` (throw), `expect`, `prefetch` (no-op), `memcpy.inline`/`memset.inline`, `memcpy`/`memset` with an `i32` length, `abs` on vectors
- [ ] The `isvolatile` flag of `memcpy`/`memset` is ignored (TODO in the intrinsic class).

## 4. Performance of generated IL

- [ ] **Promote allocas that never have their address exposed to plain locals.** `EmitStore` already uses `stloc` for primitive allocas, but loads go through `ldloca` + `ldind`, which marks the local address-exposed so the JIT won't enregister it. This dominates `-O0` code. Use `ldloc` for direct loads too, and only fall back to `ldloca` when the alloca escapes (passed to a call, stored, or used in a GEP).
- [ ] **Reuse locals.** Every non-inlined SSA value gets its own `LocalBuilder`. Large functions exceed the JIT's tracked-locals limit (`JitMaxLocalsToTrack`, default 1024), after which locals aren't enregistered. Allocate locals by (type, liveness interval) and reuse them.
- [ ] **Better `switch` lowering.** Use jump tables for dense-but-gapped ranges (see the Roslyn reference in the TODO) and binary search for sparse ones, instead of linear `beq` chains.
- [ ] **Avoid boxing in `printf`.** The generated `__printf` overloads allocate an `object[]` and box every argument on each call. Pass arguments through a `stackalloc`'d span of a tagged union struct, or generate per-format-string specialisations when the format is a constant.
- [ ] **Map function attributes** to `MethodImplOptions`: `alwaysinline` → `AggressiveInlining`, `noinline` → `NoInlining` (measure first). Give functions with `internal` linkage `private` visibility.
- [ ] **Experiment with runtimeconfig knobs** for compiled programs (`TieredPGO`, `TieredCompilation.QuickJitForLoops`, `ReadyToRun` / NativeAOT of the output) and record the effect in the benchmark suite.
- [ ] **Branchless `select`.** Scalar `select` always emits a branch diamond. For simple operands, emit a form the JIT can if-convert, or use `ConditionalSelect` for vectors (already done).

## 5. Compiler performance and code health

- [ ] **[perf] Cache reflection lookups.** Many hot emission paths call `typeof(X).GetMethodStrict(...)` per instruction (all the `Int128` operators, `Interlocked.Add`, `Unsafe.BitCast`, vector methods). Move them into `static readonly` fields or a memoizing cache.
- [ ] **[perf] `TypeSystem.GetStructFieldOffset` creates and disposes an `LLVMTargetDataRef` on every call.** Create it once per module.
- [ ] **[perf]** `CanPushToStack` materialises `GetUses().ToList()` and `GetInstructions().ToList()` per instruction. Fine for now, but compute uses once per function if profiling shows it.
- [ ] **[cleanup] Remove the duplication in `CompiledModule`.** `GetOrCreatePrintfOverload`, `GetOrCreateFprintfOverload` and `GetOrCreateSprintfChkOverload` are three copies of the same generator. Make one helper parameterised by the fixed-parameter count and target method. This also fixes:
  - the `Ldarg_S (byte)` limit (≥ 256 args)
  - unreachable switch arms in the `__sprintf_chk` version
- [ ] **[cleanup] Share the `Int128` comparison switch.** It's duplicated in `EmitICmp` and `EmitBranchCondition`. The float/double halves of `EmitConversion` are near-identical too.
- [ ] **[cleanup] Split `FunctionILEmitter.cs` (2.5k lines)** into partial-class files: arithmetic, comparisons, conversions, memory/GEP, control flow, calls, vectors.
- [ ] **[cleanup] Dead code:**
  - `FunctionILEmitter.HandleDebugDeclare` (never called; `llvm.dbg.declare` is handled by `LLVMDbgDeclareIntrinsicFunction`)
  - the `PhiInstructionAndIncomingValue` record
  - the unused `caseValueType` in `EmitSwitch`
  - commented-out blocks in `EmitIndexedPtr` and `TypeSystem`
- [ ] **[cleanup] Resolve the `LLVMSourceCode.Dispose` TODO** ("It crashes when we do this. Why?"). `LLVMParseIRInContext` takes ownership of the memory buffer, so disposing it afterwards is a double free. Remove `IDisposable` (or mark the buffer as consumed) and add a comment.
- [ ] **[cleanup]** The generated `Program` type derives from `ValueType`. Make it an `abstract sealed` (static) class.
- [ ] **[cleanup]** Drop `ConcurrentDictionary` in `TypeSystem` (single-threaded per module) or make the class genuinely thread-safe.
- [ ] **[cleanup]** `runtimeconfig.json` hard-codes `10.0.0` (TODO in `Compiler.Save`). Derive it from `Environment.Version`.
- [ ] **[ux]** `Program.Main` indexes `args[0]`/`args[1]` without validation. Add usage text, `--keep-il`/`--decompile` options, and error messages that include the offending IR instruction and source location (debug info is already available).
- [ ] **[ux]** Emit a warning when compiling something knowingly approximated (e.g. `stacksave`), rather than silently.

## 6. Platform and tooling

- [ ] **Linux support.** `IR2IL.csproj` only references `libLLVM.runtime.win-x64` and `osx-arm64`, and the clang download target only knows Windows and macOS ARM64. Add `linux-x64` (and probably `linux-arm64`, `osx-x64`, `win-arm64`) plus a Linux CI job.
- [ ] The Windows clang extraction hard-codes `C:\Program Files\7-Zip\7z.exe`. Use `tar` (bundled with Windows 10+) or document the requirement.
- [ ] Update to LLVM/LLVMSharp 21 when bindings are available. Keep the clang version used by the tests in lockstep with libLLVM.
- [ ] **Documentation.** `README.md` is one line. Add how to build, how to run the CLI, the supported subset of IR, the platform limitations (varargs, stdio model) and a pointer to this file. Several CLAUDE.md sections (arbitrary-width integers, varargs) are good starting material for a `docs/architecture.md`.
