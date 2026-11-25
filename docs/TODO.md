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
- [x] **Avoid one `dotnet` process start per test.** Run compiled assemblies in a small pool of long-lived worker processes, with the assembly loaded into a collectible `AssemblyLoadContext` and stdout/stderr redirected. Fall back to a fresh process for tests that call `exit`/`abort`. **Decided not to do:** the worker pool, descriptor-level output capture and fallback logic add too much complexity for the benefit.
- [x] **`Benchmark` stays in the default run, as a correctness test only.** The tests assert that output and exit code match the native binary, so they give useful coverage on larger, more realistic code, and they cost only ~2% of total test time. They share their body with `Arbitrary` and no longer time anything: timings come from `src/IR2IL.Benchmarks` (BenchmarkDotNet).
- [x] **CI runs the whole suite once, in Release only.** Debug was dropped: the IL under test comes from `Reflection.Emit`, not from the build configuration, and Debug only adds `Debug.Assert`s in `IR2IL.Runtime` and different optimisations, which is marginal value for doubling the CI time. Run Debug locally if needed.
- [x] **Add a timeout to `RunProgram`** and kill the process tree on expiry. Today a miscompiled infinite loop hangs the whole run.
- [x] Moved to MSTest 4.5.3 with Microsoft.Testing.Platform (`MSTest.Sdk`, opted in via `global.json`). Measured on the 414-test `CTestSuite`: run time unchanged (~6 s), discovery ~0.4 s faster. Kept for the future direction of .NET testing and extensions such as `--hangdump`.

### Coverage and visibility
- [ ] **Add small hand-written `.ll` tests per instruction/intrinsic.** `HandWrittenIRTests.cs` builds every `tests/hand-written/*.ll` file twice, with IR2IL and with clang, and compares exit code, stdout and stderr, so a test needs no expected value: it prints its results or returns a failing code. A file must avoid undefined behaviour and poison, since clang and IR2IL may legitimately differ there. Most files are written by a script in `tests/generators/`. A test that exposes a bug is not committed until the bug is fixed.
  - [x] Harness: clang as the oracle; the `; exit-code:` comments are gone
  - [x] Integers: `i1`/`i8`/`i16`/`i24`/`i40` crossed with every consumer, `i65`+ unnormalised, exact-width loads and stores, narrow constants, `inttoptr`, GEP with a narrow index
  - [x] Vector `icmp` (all integer predicates), mask flow, padded lanes, `zext` of `sbyte` lanes
  - [x] Memory ordering: load/store reordering, `memcpy`, `switch` on an inlined expression
  - [x] Scalar `float`/`double` arithmetic (`fadd`, `fsub`, `fmul`, `fdiv`, `frem`, `fneg`): `float_arithmetic.ll`
  - [x] Scalar float conversions (`fptosi`, `fptoui`, `fpext`, `fptrunc`): `float_conversions.ll`
  - [x] Scalar `fcmp`, all 16 predicates stored, as a `br` and as a `select` condition: `float_fcmp.ll` (found and fixed missing predicates and NaN in `oge`/`ole`/`uge`/`ule`)
  - [x] Vector `fcmp` predicates: found and fixed (all 16, with NaN)
  - [x] `vector_float_fcmp.ll`, `vector_float_arithmetic.ll` and `vector_float_conversions.ll` pass (all 16 `fcmp` predicates, `fadd`/`fsub`/`fmul`/`fdiv`/`frem`/`fneg`, every float/integer conversion, over 2 to 16 lanes of `float` and `double` including the padded `<3 x float>`)
  - [x] Scalar and vector `sitofp`/`uitofp` over their full ranges: from `i8`/`i16`/`i32`/`i64` to `float` and `double` (2 to 16 lanes, and `<3 x i32>`), over the limits, powers of two and the values around the ties that must round to even (`float_conversions.ll`, `vector_float_conversions.ll`; found the double rounding of vector `i64` to `float` once the pin moved to LLVM 21)
  - [x] Vector `shl`/`lshr`/`ashr` by a vector of different counts, over `i8` to `i64` lanes and 2 to 32 lanes: `vector_variable_shifts.ll` (ARM64 uses `USHL`/`SSHL`; the AVX2 and AVX-512 paths in `VectorUtility.TryShiftWithHardware` have never been run, as the machine they were written on is ARM64, so check that CI covers them)
  - [x] GEP and aggregates: `gep_aggregates.ll` (struct, nested, multidimensional and variable-index GEPs, constant-expression GEPs, negative offsets, `ptrtoint`/`inttoptr`) and `aggregate_values.ll` (`extractvalue`/`insertvalue` on structs and arrays, nested, with several indices). Found and fixed: negative constant GEP offsets were zero-extended, `insertvalue` on structs and multi-index `extractvalue`/`insertvalue` threw, and `extractvalue` of an inlined or constant aggregate had no address
  - [ ] `atomicrmw` (every operation), `unreachable`, calls with mixed argument types, globals and constant expressions:
    - [x] Integer `atomicrmw` (`xchg`, `add`, `sub`, `and`, `nand`, `or`, `xor`, `max`, `min`, `umax`, `umin` over `i8`/`i16`/`i32`/`i64`): `atomicrmw_integers.ll`, written by `tests/generators/generate_atomicrmw_tests.py` (every pair of limits and bit patterns, constant operands `1` and `-1`, an unused result, and every ordering, `volatile` and `syncscope` on `i32 add`). Each case prints the returned old value and the value left in memory. Before, only `add` on `i32`/`i64` was implemented, and it returned the new value instead of the old one. Now `EmitAtomicRMW` calls `Interlocked` directly where it has a method (`Exchange`, `And` and `Or` on every width, via the generic `And<T>`/`Or<T>` for 8 and 16 bits; `Add` and `Sub` on 32 and 64 bits, which return the new value, so the emitter subtracts or adds the operand back), and uses `AtomicHelper.ReadModifyWrite<T, TOperation>` (`src/IR2IL.Runtime/AtomicHelper.cs`), a compare-exchange loop instantiated per operation and type, for the rest (`add`/`sub` on 8 and 16 bits, `xor`, `nand`, `max`, `min`, `umax`, `umin`). `i128` and non-power-of-two widths still throw (no `Interlocked` support; `i128` also needs `cmpxchg16b` or a libcall). The same rule applies to the floating point operations: `Interlocked` where it exists (`Exchange` on `float`/`double`), a compare-exchange loop otherwise
    - [x] Floating point `atomicrmw` (`xchg`, `fadd`, `fsub`, `fmax`, `fmin` on `float`/`double`): `atomicrmw_floats.ll`, from the same generator (every pair of the values of `float_arithmetic.ll`, constant operands `1.0` and `-1.0`, an unused result; 6100 lines of output). `xchg` is `Interlocked.Exchange`; the rest are `AtomicHelper.ReadModifyWriteFloat<T, TOperation>`, a compare-exchange loop that compares bit patterns (a NaN never equals itself). `fmax`/`fmin` are `T.MaxNumber`/`MinNumber` (`llvm.maxnum`/`minnum`: a NaN operand gives the other one), and their test table leaves out -0.0, because LLVM may return either zero for +0.0 against -0.0. `half`, `bfloat` and vectors of floats still throw
    - [ ] `unreachable`: a never-taken branch to it, and blocks that end in it after a `noreturn` call
    - [ ] Calls with mixed argument types: `i8`/`i16`/`i64`/`float`/`double`/`ptr`/vector/struct by value, as constants, loaded values and call results; every return type; `byval`/`sret` if clang emits them
    - [ ] Globals (initialised, zero-initialised, constant, arrays, structs, pointers to other globals, each linkage) and constant expressions (GEP, `ptrtoint`/`inttoptr`, and `add`/`sub`/`trunc`/`bitcast`/`select`/`icmp`, which §3 lists as unsupported: fix the cheap ones and leave the rest in the TODO)
  - [ ] Intrinsics, one generated test per family, across their overloads:
    - [ ] math (`sqrt`, `sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `atan2`, `sinh`, `cosh`, `tanh`, `exp`, `log`, `log10`, `pow`, `fabs`, `ceil`, `copysign`, `fmuladd`, `is.fpclass`)
    - [ ] integer (`abs`, `smin`/`smax`/`umin`/`umax`, `scmp`, `ctpop`, `usub.sat`)
    - [ ] vector reductions (`vector.reduce.add`/`mul`/`fadd`/`smax`)
    - [ ] memory (`memcpy`, `memmove`, `memset`)
    - [ ] stack and varargs (`stacksave`/`stackrestore`, `va_start`) and the housekeeping ones (`lifetime`, `dbg`, `assume`, `objectsize`, `threadlocal.address`)
- [ ] **Expand the Fujitsu suite beyond directory `0020`.** Only about 2.6k of its 21k C files are used (`<= 0020` filter). Add directories one at a time and track the pass rate per directory.
- [ ] **Make excluded tests visible.** Move the large `switch` ignore lists in `CompilerTests.cs` into a data file (test id, platform, reason). Report excluded tests as `Inconclusive`/skipped rather than silently dropping them, so tests that start passing get noticed and the reasons stay reviewable.
- [ ] **Re-check stale exclusions.** Several ignore comments mention MSVC-specific problems but apply on all platforms (e.g. `0005_0005`, `0005_0006`, `0014_*` bitfields, `0016_*`/`0018_*` `_Complex`), so they could run on macOS. Similarly, `0021_*` (`extern int printf(...)`) is excluded everywhere but the comment is Windows-specific.
- [ ] Run tests at `-O1`/`-O2`/`-Os` as well as `-O0`/`-O3` once the suite is faster; each level exercises different IR shapes.

## 2. Correctness

### Instruction scheduling / stack inlining
- [x] **[bug] Expressions that read memory could be evaluated after a store.** `CanPushToStack` only guarded an inlined instruction that was itself a load, so in
  ```llvm
  %v = load i32, ptr %p
  %a = add i32 %v, 1      ; single use, further down
  store i32 0, ptr %p
  call void @use(i32 %a)
  ```
  `%a` (and so `%v`) was emitted at the call, *after* the store. Intrinsic calls such as `memcpy`, `memset` and `va_start` were also treated as not writing memory. Fixed: `LLVMExtensions.GetMemoryEffect` classifies each instruction as `None`/`Reads`/`Writes` (only intrinsics that report `IsPure` are `None`), and `CanPushToStack` takes the strongest effect over the instruction and the operands inlined into it. Regression tests are in `HandWrittenIRTests.cs`. The other pure opcodes (`sub`, `mul`, `trunc`, `select`, casts, …) are deferrable too, except when they produce an integer narrower than 32 bits (see the next item).
- [x] **[perf/bug] `EmitSwitch` re-emitted the condition once per case cluster.** If the condition was inlined (`CanPushToStack`), its whole expression was recomputed for each cluster. It is now evaluated once into a local when there is more than one cluster (`tests/hand-written/switch_on_inlined_expression.ll`).

### Integer semantics
- [x] **[bug] `i1`/`i8`/`i16` values on the IL stack weren't normalised.** A `load` sign-extends but `trunc` (`conv.u1`) zero-extends, and `icmp` compared the raw stack values, so `icmp eq i8 (load), (trunc …)` was wrong for values ≥ 0x80 whenever neither side was stored to a local. `EmitSwitch` had the same bug for a single-cluster `switch`. Fixed by making consumers, not producers, responsible: `EmitIntegerOperand`/`EmitNormalizedInteger` extend an operand from its logical width (sign- or zero-extending as the consumer needs) in every instruction that depends on the upper bits, for every width up to 64 bits, and the masks after `trunc` and arithmetic are gone (see CLAUDE.md, "Normalisation"). That also made the `GetMemoryEffect` workaround unnecessary. Same change fixed `sext`/`sitofp`/signed `icmp`/`sdiv`/`srem`/`ashr` on non-power-of-two widths up to 64 bits (`sext i1 true to i32` was 1), scalar `icmp ule`, the missing `uge` branch-fused predicate, `inttoptr` from a narrow integer (it sign-extended), and GEP indices narrower than `i64` (they threw). Tests: `tests/hand-written/narrow_integers_i*.ll`, written by `tests/generators/generate_narrow_integer_tests.py`, `narrow_icmp_load_vs_trunc.ll`, `gep_narrow_index.ll`, `inttoptr_narrow.ll`.
- [x] **[bug] Signed operations on non-power-of-two widths** (up to 64 bits). `i1`/`i40` are held zero-extended in their container, so `sext`, `sitofp`, `icmp slt/sgt/sle/sge`, `sdiv`, `srem` and `ashr` saw a positive number when the logical sign bit was set (`sext i1 true to i32` was 1, not -1). `EmitNormalizedInteger` now sign-extends from bit *N-1* (`shl`/`shr` by the unused bits) before each of them. Wider than 64 bits (`Int128`) is still not handled. Covered by the `narrow_integers_i*.ll` tests.
- [x] **[bug] Loads and stores of non-power-of-two widths access too many bytes.** `EmitLoadIndirect`/`EmitStoreIndirect` rounded up to the container (`i24` → `ldind.i4`/`stind.i4`, `i40` → 8 bytes), so stores overwrote the bytes after the object and loads could read past the end of an allocation. Both now move exactly `ceil(N/8)` bytes: `EmitStoreIntegerIndirect` spills the value and `cpblk`s the low bytes; `EmitLoadPartialInteger` `cpblk`s them into a zeroed local (little-endian only). No masking is needed, because consumers normalise (see CLAUDE.md, "Normalisation"). Widths above 64 bits already loaded `ceil(N/8)` bytes through `LoadWideInt`. Tests: `tests/hand-written/store_exact_width.ll`, `load_exact_width.ll`.
- [x] **[bug] Scalar `icmp ule` (outside a branch) threw `NotImplementedException`.** `EmitICmp` now emits it as `cgt.un` followed by a negation. Covered by `narrow_integers_i*.ll`.
- [ ] **[bug] Unsigned vector comparisons use signed element types.** `GetMsilVectorElementType` maps integers to signed types (`sbyte`, `short`, …), and `UGT`/`ULT` call `Vector128.GreaterThan`/`LessThan` on them, which is wrong when the top bit is set. Reinterpret as the unsigned type (`AsByte`, `AsUInt32`, …) first. Vector `SLE`, `UGE`, `ULE` are missing entirely.
- [x] **[bug] Negative constant GEP offsets are zero-extended.** `ILEmitter.EmitConstantValue` (const-expr GEP) emits `ldc.i4 <offset>; conv.u`; `conv.u` zero-extends an `int32`, so `-8` becomes `0xFFFFFFF8`. Use `conv.i` (or `ldc.i8`). `GetElementPtrConst` also returns `int`, so offsets ≥ 2 GiB overflow.
- [x] **[bug] `EmitIndexedPtr` threw for variable GEP indices that are not `i64`.** It now sign-extends the index (`EmitIntegerOperand`), whatever its width. Covered by `tests/hand-written/gep_narrow_index.ll`.
- [ ] `sdiv`/`srem` with `INT_MIN / -1` and division by zero throw .NET exceptions. That's UB in C, so acceptable, but document it.

### Floating point
- [x] **[bug] Vector conversions and `frem` needed a helper per shape.** The emitter named a `VectorUtility` method from the opcode and the two types (`FPToSIV4F32ToV4I32`) and failed with `Method X not found` for any shape nobody had written. Now `FunctionILEmitter.VectorConversions.cs` composes every `sext`, `zext`, `trunc`, `fpext`, `fptrunc`, `fptosi`, `fptoui`, `sitofp` and `uitofp` from `WidenLower`/`WidenUpper`/`Narrow` (a lane width change of 2), `ConvertTo*` (a change of kind at one lane width) and `As`, emitted inline, and the mask resizing uses the same steps. The 68 per-shape conversion helpers are gone. Vector `srem`/`urem`/`frem` call one generic `VectorUtility.Remainder<TVector, TLane>`, a loop over the real lanes (there is no SIMD division, and the padding lanes may hold a zero divisor). `inttoptr`/`ptrtoint` of vectors still have a helper per shape.
  - A vector `sitofp`/`uitofp` from 64-bit integers to `float` is converted one lane at a time (`VectorUtility.ConvertInt64ToSingle`/`ConvertUInt64ToSingle`), because .NET has no `ConvertToSingle` for 64-bit lanes and going through `double` rounds twice (`2^60 + 2^36 + 1` became `0x5d800000`, a single rounding gives `0x5d800001`). LLVM 20 on ARM64 did the same double rounding, which hid it from the clang-oracle tests until the pin moved to LLVM 21; `tests/hand-written/vector_i64_to_float_rounding.ll` covers it. On x64 with AVX-512 DQ the 2-, 4- and 8-lane shapes use `vcvtqq2ps`/`vcvtuqq2ps` instead (single rounding too). **That path has never been run**: it needs an AVX-512 machine, and the ARM64 machine it was written on has none.
- [ ] **[bug] `undef`/`poison` of `double` and `i128` types throw** in `EmitConstantValue` (only `float` and integers up to 64 bits are handled).
- [ ] **[gap]** `half`, `bfloat`, `fp128`, `x86_fp80` types (`0022_*` Fujitsu tests are excluded for `half`).

### Calls, function pointers and the C runtime
The varargs and stdio items below (`fprintf`, output ordering, `sprintf` family, and `va_arg` in §3) are addressed together by [varargs-and-stdio.md](varargs-and-stdio.md).

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
- [ ] `cmpxchg`, `fence`, and `atomicrmw` on `i128`, on non-power-of-two widths, on `half`/`bfloat`/vectors, on pointers (`xchg`), and `uinc_wrap`/`udec_wrap`/`usub_cond`/`usub_sat` (the integer operations on 8 to 64 bits and the `float`/`double` ones are done)
- [ ] `load atomic`/`store atomic` orderings, and `volatile` loads/stores (`volatile.` prefix). Alignment below the natural alignment should emit the `unaligned.` prefix.
- [ ] `va_arg` (and a portable `va_start`: the current one relies on CoreCLR's `arglist` layout and doesn't work on macOS ARM64)
- [ ] `addrspacecast`, `indirectbr` / `blockaddress`, `callbr`
- [ ] C++ exceptions: `invoke`, `landingpad`, `resume` (map to `try`/`catch` regions with a runtime exception type carrying the C++ exception pointer)
- [x] `extractvalue`/`insertvalue` with multiple indices; `insertvalue` on **structs** (`aggregate_values.ll`)
- [ ] `switch` on conditions wider than 32 bits
- [ ] General `shufflevector` lowering via `Vector128.Shuffle` for single-source in-register shuffles (currently anything that isn't a splat or a concatenation goes through memory element by element)

### Constant expressions
- [ ] Only `getelementptr`, `inttoptr` and `ptrtoint` are supported. Add `sub`/`add` (pointer differences between globals, common in tables), `trunc`, `bitcast`, `select` and `icmp`, ideally by calling LLVM to fold them first.

### Intrinsics
- [ ] **[cleanup] Resolve intrinsics by pattern instead of exact name.** `IntrinsicFunctions.LLVMIntrinsics` lists each overload by its mangled name (`llvm.sin.v2f32`, `llvm.sin.v4f32`, …), and `VectorUtility` still has about 40 hand-written per-width helpers (math functions, `inttoptr`/`ptrtoint`, `MaskFromBits`). Parse the name into base name + overload types and dispatch to a generic implementation (`Vector128.Sin<T>`, or element-wise fallback through `Vector*<T>` generics). That would remove most of the table and most per-width helpers.
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
- [ ] **Consider a per-basic-block scheduler to reduce register pressure.** RyuJIT allocates in IL order and can't reschedule, while clang's backend does, so large unrolled loop bodies (`benchmark_nbody` at O3: 27 loop-carried values, an estimated peak of about 72 live values against 32 SIMD registers) spill heavily (187 of 426 loop instructions are `ldr`/`str`, against 38 of 260 natively). Hand-reordering nbody's IR to an estimated peak of 35 made the managed code about 10% faster fully optimised and about 23% faster with default tiered PGO, which also removed most of the PGO slowdown. One benchmark and one hand-run scheduler, so measure before committing.
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
- [ ] Update to LLVM/LLVMSharp 22 or later. Pinned now: LLVM 21.1.8 (clang, `libLLVM.runtime.*`) and LLVMSharp.Interop 21.1.8.1, kept in lockstep. The `libLLVM.runtime.osx-arm64` packages for 22.1.8 and 23.x are Homebrew builds that link to `/opt/homebrew/opt/llvm` and `z3`, so they don't load without those; 23 also splits `LLVMBr` into `LLVMCondBr`/`LLVMUncondBr`.
- [ ] **Documentation.** `README.md` is one line. Add how to build, how to run the CLI, the supported subset of IR, the platform limitations (varargs, stdio model) and a pointer to this file. Several CLAUDE.md sections (arbitrary-width integers, varargs) are good starting material for a `docs/architecture.md`.
