# Rust support: plan

Status: proposed (2026-11-24). Nothing here is implemented yet. The experiments below were run on macOS ARM64 with rustc 1.100.0 (LLVM 23.1.5); the scratch files were in `/tmp/rs`.

The goal is to compile a Rust `fn main() { println!("Hello, world!"); }` with IR2IL and run it on the CLR, then grow from there.

## Current state

`rustc --emit=llvm-ir` on a hello-world gives IR for the user crate only (150 lines at `-O0`). IR2IL does not handle it:

| Program | Result |
|---|---|
| `println!` hello-world (`-O0` and `-O3`) | Compiler crash: `Unexpected value: ptr asm sideeffect "", "~{memory}"`. |
| `#![no_std]`/`#![no_main]` calling libc `puts` | Compiles and runs, printing `Hello, world!`. |

The IR is mostly startup plumbing. `@main` calls `std::rt::lang_start`, which calls `lang_start_internal` (declared only), which runs `__rust_begin_short_backtrace(hello::main)` and then `Termination::report`. `hello::main` builds a `fmt::Arguments` for the string and calls `std::io::stdio::_print` (declared only).

### Inline asm (`black_box`)

The asm comes from `std::hint::black_box`, used in `__rust_begin_short_backtrace` right after the call to `main`:

```llvm
call void asm sideeffect "", "~{memory}"()
```

The assembly text is empty and `~{memory}` only says that memory may be touched, so LLVM treats it as an optimization barrier. Here it stops the call to `main` from becoming a tail call, which would remove the frame that backtraces use as a marker. It does nothing at runtime, so IR2IL can skip a call whose asm string is empty.

### The std library is not in the IR

Rust's std is precompiled. The user crate's IR only declares `lang_start_internal` and `_print`, and IR2IL would turn them into P/Invokes against `libc`, where they do not exist.

## Getting std's IR

The std rlibs ship with embedded bitcode (a `__bitcode` Mach-O section, about 5 MB in `libstd`). No `-Zbuild-std` and no `rust-src` component is needed.

### rustc flags

| Command | Result |
|---|---|
| `--emit=llvm-ir -C lto=fat` (or `--emit=llvm-bc`) | 15 functions: still only the user crate, because `--emit` writes the module before LTO. |
| `-C lto=fat -C save-temps` | The whole program as bitcode: `<crate>.<crate>.<hash>-cgu.0.rcgu.bc` has 2479 functions at `-O0` and 443 at `-O3` (LTO drops unreachable code). |

The merged module contains the bodies of `lang_start_internal` and `_print`. What stays external is libc, pthread and dyld calls (`malloc`, `write`, `mmap`, `pthread_mutex_*`, `_tlv_atexit`, `_dyld_*`, `_Unwind_Backtrace`, about 270 declarations in all).

`save-temps` leaves several other files next to the output, so the wrapper below should use `--out-dir` and pick up the `.rcgu.bc`.

### LLVM version mismatch

rustc writes LLVM 23 bitcode and IR2IL's LLVMSharp bundles LLVM 20.1.2. Reading the bitcode directly fails with `Unknown attribute kind (103)`. The conversion that worked on std (using `llvm-dis` and `opt` from `rustup component add llvm-tools`, which match rustc's LLVM):

1. `opt --strip-debug --strip-named-metadata` to drop debug info (25 MB of text becomes 10 MB).
2. `opt -passes='lower-invoke,simplifycfg'` to turn every `invoke` into a plain call. std is built with unwinding, and IR2IL has no `invoke`; this treats panics as abort.
3. `llvm-dis` to text IR.
4. `sed` to remove syntax that LLVM 20 rejects: ` captures(...)`, ` dead_on_return`, `target_mem: x` and `errnomem: x` inside `memory(...)`, and ` nocreateundeforpoison`.

Not yet checked: whether the LTO'd module still has `invoke`s when the user crate uses `-C panic=abort`.

## What IR2IL is missing

A scratch copy of the compiler that logs the first failure of each function and carries on, run on the std module alone (1354 functions, all of std's non-generic code). 498 functions failed and 856 emitted. Later failures may be hidden behind the first one in each function.

| Count | Missing feature |
|---|---|
| 223 | Unknown intrinsics: `llvm.umin` (130), `is.constant` (16), `bswap` (16), `trap` (11), `sadd`/`ssub`/`umul.with.overflow` (15), `ptrmask` (7), `cttz`/`ctlz`/`ctpop` (14), `usub.sat` (5), `ucmp` (5), `fshl` (4). |
| 106 | `switch` on a condition wider than 32 bits (Rust switches on `i64` discriminants). `EmitSwitch` accepts at most `i32`. |
| 88 | `insertvalue` on a struct. `EmitInsertValue` only handles arrays. |
| 55 | `atomicrmw sub`. |
| 21 | `cmpxchg` (not implemented at all). |
| 5 | A call returning a struct used as an address; an `i128 undef` constant. |

The hello-world itself needs the inline asm and struct `insertvalue` first. Saving the output also crashes with `Label N has not been marked` when functions failed part-way; that is a consequence of the failures, not a separate bug.

`libcore` and `liballoc` are separate rlibs, but with LTO their code arrives in the same module, so the numbers above will grow when the whole program is compiled.

## Plan

### 1. Make the hello-world compile

- [ ] Skip calls to inline asm with an empty asm string and only a memory clobber; fail with a clear message for any other asm.
- [ ] `insertvalue` on struct aggregates (and `extractvalue`, if it has the same limit).
- [ ] `switch` on `i64` (a compare chain or a split into 32-bit ranges, keeping the existing clustering for `i32`).
- [ ] `atomicrmw sub` and `cmpxchg` (and `cmpxchg weak`, which can behave as the strong form).
- [ ] The intrinsics in the table, registered in `IntrinsicFunctions.cs`. Most are `StandardIntrinsicFunction.Create` on `Math`, `BitOperations` or `Int128`, or small helpers in `LLVMIntrinsics.cs`. `llvm.trap` can throw, `llvm.is.constant` can be `false`, `llvm.ptrmask` is an `and`.
- [ ] Make the compiler report all failing functions in one run instead of the first one, behind an option. This was the most useful tool in the survey.

### 2. Make the std runtime work on the CLR

- [ ] Decide how the libc, pthread and dyld symbols resolve. IR2IL already turns external declarations into P/Invokes against `libc`; check `pthread_*`, `_tlv_atexit`, `_dyld_*`, `dispatch_semaphore_*` and `__error` on macOS.
- [ ] Thread-local storage (`thread_local` globals in std).
- [ ] Panic strategy: with `lower-invoke`, panics abort. Decide whether that is acceptable or whether `invoke`/`landingpad` should map to .NET exceptions.
- [ ] Run the hello-world and check stdout ordering and the exit code.

### 3. A user-facing command

A wrapper, for example `ir2il-rustc hello.rs -o hello.dll`, that:

1. Runs `rustc -C lto=fat -C save-temps -C panic=abort --out-dir <tmp>`.
2. Picks up the `.rcgu.bc` file.
3. Normalises it for IR2IL's LLVM.
4. Calls `Compiler.Compile`.

The normalisation (step 3 of "LLVM version mismatch") should live in the wrapper or in IR2IL itself, so users never run `llvm-dis` and `sed` by hand. The longer-term fix is an IR2IL built against a newer LLVM; the version in use here is two major releases behind rustc.

### 4. Tests

- [ ] A test that compiles a small Rust source with rustc and runs it, skipped when rustc is not installed (like the clang tests, caching the output).
- [ ] Hello-world as the first case, then `println!` with arguments, `Vec`, `String` and a `panic!` test as the std dependency grows.

## Open questions

- Is `-C panic=abort` on the user crate enough to remove unwinding from the LTO'd module, or is `lower-invoke` always needed?
- Should IR2IL depend on the `llvm-tools` rustup component for the conversion, or ship its own downgrade step?
- Is the LTO'd module (hundreds to thousands of functions) acceptable to compile on every run, or should the std part be compiled once and cached as a separate assembly?
