# Plan: replace LLVMSharp with our own bitcode reader

Status: proposed, 2026-11-24.

## Goal

Remove the `LLVMSharp.Interop` and `libLLVM.runtime.*` packages. IR2IL reads LLVM **bitcode** (`.bc`) with its own reader and builds its own class hierarchy (`Module`, `Function`, `BasicBlock`, `Instruction`, `Type`, `Constant`, …). Users pass a `.bc` file; textual `.ll` is no longer accepted.

Why this is possible: IR parsing is the only thing we use LLVM for. We never emit native code through it. Beyond parsing, we use a handful of other native calls, listed below, and all of them have to be reimplemented.

### What we use from LLVMSharp today

| Area | Calls | Replacement |
|---|---|---|
| Parsing | `ParseIR`, `CreateMemoryBufferWithContentsOfFile` | Bitstream reader and module reader |
| Data layout | `CreateTargetData`, `SizeOfTypeInBits`, `OffsetOfElement`, `GetModuleDataLayout` | Own `DataLayout` (parse the datalayout string) |
| Use lists | `GetUser`, `GetNextUse` (`GetUses`) | Computed after reading each function |
| Instruction details | `GetGEPSourceElementType`, `GetAllocatedType`, `GetCalledFunctionType`, `GetNumMaskElements`/`GetMaskValue`, `GetIndices`/`GetNumIndices`, `GlobalGetValueType` | Properties on the typed instruction classes |
| Debug info | `InstructionGetDebugLoc`, `DILocation*`, `DIScopeGetFile`, `DIFile*`, `DILocalVariable` name and arg, `GetMDKindID`, `GetMetadataKind`, `ValueAsMetadata`/`MetadataAsValue` | Minimal metadata reader (see Phase 3) |
| Everything else | `LLVMValueRef`/`LLVMTypeRef`/`LLVMOpcode`/`LLVMTypeKind`/… used throughout `FunctionILEmitter`, `TypeSystem`, `ILEmitter`, `CompiledModule`, `ModuleCompiler`, `GlobalsILEmitter`, `Intrinsics/*`, `LLVMExtensions` | The new model |

## Decisions (agreed)

- **Typed hierarchy, in the style of LLVM's C++ classes**: `Value` → `User` → `Instruction` → `LoadInst`, `StoreInst`, `GetElementPtrInst`, …; `Constant` → `ConstantInt`, `ConstantFP`, `ConstantExpr`, …; `Type` → `IntegerType`, `PointerType`, `StructType`, …. `Instruction.Opcode` is still an enum, so existing `switch (opcode)` code ports easily and can then use the typed subclass in each case.
- **Debug info is kept**: sequence points and parameter names keep working.
- **Phased, with differential tests**: the new reader is checked against LLVMSharp over the whole test corpus before the emitter is ported.
- **The differential dumper is temporary.** Once LLVMSharp is gone, the structural dump code and its tests are deleted. We do not maintain them.
- **Scope is parity** with what the emitter supports today, but the reader must make new instructions, constants, types and metadata kinds cheap to add (see "Extensibility").

## Where the code lives

A new project, `src/IR2IL.Bitcode` (namespace `IR2IL.Bitcode`). It has no dependency on `System.Reflection.Emit` or on the rest of IR2IL, so the boundary stays clean and the reader is testable on its own. `IR2IL` references it. During the migration the test project references both it and LLVMSharp (for the differential tests).

```
IR2IL.Bitcode/
  Bitstream/    BitReader, Abbreviation, BlockReader, BitstreamCursor
  Model/        Module, Function, BasicBlock, Argument, GlobalVariable, GlobalValue,
                Value, User, Use, Type*, Constant*, Instruction*, Metadata*
  Reader/       ModuleReader (+ TypeTableReader, ConstantsReader, MetadataReader,
                FunctionReader, SymbolTableReader), BitcodeCodes (enums of record codes)
  DataLayout.cs
```

## Input format notes

Relevant facts about the format, which the reader has to handle:

- Magic `BC 0xC0DE`. An optional wrapper header (`0x0B17C0DE`) is skipped but not otherwise needed.
- Bitstream: abbreviation IDs of variable width (2 bits at top level), `ENTER_SUBBLOCK`, `END_BLOCK`, `DEFINE_ABBREV`, `UNABBREV_RECORD`, fixed, VBR, array, char6 and blob operands, and `BLOCKINFO` (which defines abbreviations for other blocks).
- Blocks we need: `IDENTIFICATION` (13), `MODULE` (8), `TYPE_NEW` (17), `CONSTANTS` (11), `FUNCTION` (12), `VALUE_SYMTAB` (14, for local names), `METADATA` (15), `METADATA_ATTACHMENT` (16), `METADATA_KIND` (22), `STRTAB` (23). Blocks we skip by length: attributes (9, 10), `USELIST` (18), `OPERAND_BUNDLE_TAGS` (21), `SYNC_SCOPE_NAMES` (26), `SYMTAB` (25), summary blocks.
- Global and function names are `(offset, size)` pairs into the `STRTAB` blob, which comes *after* the module block, so names are resolved at the end.
- Value numbering: globals, functions and aliases first, then module-level constants, then per-function arguments, function-level constants and instructions. Operands in function blocks are **relative** IDs (current value number minus operand), and a forward reference carries an explicit type. The reader needs a forward-reference placeholder that gets resolved when the value is defined.
- Function bodies appear in the order of the module's non-prototype function records.
- Opaque pointers only (LLVM 17+). Typed-pointer bitcode is out of scope and gives a clear error.
- Compatibility: LLVM only ever adds trailing record fields and keeps old record forms readable. So the reader reads the fields it knows and ignores extra trailing ones. The local Apple clang 21 writes the LLVM 21 format and `build/llvm` holds clang 20; both are tested.

## Phases

Each phase ends with the full suite green (`dotnet test src`, 5,022 tests) and is a separate commit or set of commits.

### Phase 0: switch inputs to `.bc` while LLVMSharp is still in place

LLVMSharp's `ParseIR` accepts bitcode, so this is independent of the new reader and removes the input-format risk first.

- [ ] Tests and benchmarks produce `.bc` with `clang -c -emit-llvm` (and a `.ll` next to it for debugging only). The clang cache (`build/cache/clang`) stores both.
- [ ] Hand-written tests: `HandWrittenIRTests` converts each `tests/hand-written/*.ll` with `clang -c -emit-llvm x.ll -o x.bc` (cached) before compiling.
- [ ] `Compiler`, `ModuleCompiler` and the CLI take a `.bc` path; update the README and usage text.
- [ ] Check that the native-binary oracle paths still use the `.ll`/source as before.

### Phase 1: bitstream reader

- [ ] `BitReader` over `ReadOnlyMemory<byte>`: fixed, VBR (32 and 64-bit), alignment to 32 bits, char6.
- [ ] Abbreviation definitions and `BLOCKINFO`; block enter/exit with length checks; generic record cursor that yields `(code, operands[], blob)` for any block.
- [ ] Unit tests on small hand-built streams plus a few real `.bc` files: the block tree of a file is walked end to end with no unread bits.

### Phase 2: object model and core reading

- [ ] Types: void, integer (any width), half/float/double (`x86_fp80` etc. are reported as unsupported, not misread), pointer (with address space), array, vector (fixed; scalable is an error), struct (named, literal, packed, opaque), function, label, metadata. Named structs may refer to each other; resolve by creating all type entries before filling bodies.
- [ ] `Module`: datalayout, triple, globals, functions, named struct types, source file name.
- [ ] Globals and functions: linkage, constness, thread-local, initializer, value type, parameters (with names from `VALUE_SYMTAB`), `isDeclaration`.
- [ ] Constants: int (including > 64 bits: `APInt`-style word list → `BigInteger`/`Int128` accessors), FP (bit pattern), null, undef, poison, aggregate zero, struct/array/vector, data array/vector (`CST_CODE_DATA`, with `CST_CODE_STRING`/`CSTRING` as byte arrays), constant expressions (`getelementptr`, casts, and the other binary and compare forms the bitcode can contain, read generically so adding one is cheap), plus `CST_CODE_SETTYPE` handling.
- [ ] `DataLayout`: parse the datalayout string (`e`/`E`, `m`, `p[n]`, `i`, `f`, `v`, `a`, `n`, `S`, `Fn`, …), with LLVM's defaults. Provide `GetTypeSizeInBits`, `GetTypeAllocSize`, `GetABITypeAlign`, `GetStructLayout`/`GetElementOffset`. Match what `LLVM.SizeOfTypeInBits` and `OffsetOfElement` return today, including `i128` alignment and padding.
- [ ] Instructions: all opcodes the emitter handles (`ret`, `br`, `switch`, `unreachable`, the binary operators and casts, `icmp`/`fcmp` with predicates, `phi`, `select`, `alloca`, `load`/`store` (with `volatile` and atomic ordering fields), `getelementptr`, `call` (with function type, calling-convention flags, tail-call kind), `extractvalue`/`insertvalue`, `extractelement`/`insertelement`/`shufflevector`, `freeze`, `atomicrmw`). `fneg` is the unary form. Each has a typed class with the properties the emitter reads today (`SourceElementType`, `AllocatedType`, `CalledFunctionType`, `Indices`, `ShuffleMask` with undef as −1, `Predicate`, `IncomingValues`, `SwitchDefault`/`Cases`, `Successors`).
- [ ] Value numbering with forward references (including `phi` operands, which are signed VBR relative IDs), basic block creation from `DECLAREBLOCKS`, instruction names from the function-level `VALUE_SYMTAB`.
- [ ] Use lists: after a function is read, fill `Value.Users` / `Uses` and keep them consistent with what `GetUses` returned (see Open questions about ordering).
- [ ] Unknown instruction record codes throw `NotSupportedException` naming the record code. A missing instruction must never be silently skipped.

### Phase 3: metadata and debug info

Only what the emitter reads: sequence points and parameter names.

- [ ] `METADATA_KIND` block (the `dbg` kind ID and the other fixed kinds) and `METADATA_STRINGS` (bulk blob with an offsets array).
- [ ] Node records for `DILocation`, `DISubprogram`, `DILexicalBlock` (and `…File`), `DIFile`, `DILocalVariable` (name, arg, scope), and `DIGlobalVariable*` only if something needs them; every other metadata record is stored as an `OpaqueMetadata` node with its record code, so references to it resolve and nothing fails.
- [ ] Function-level `METADATA` blocks, `METADATA_ATTACHMENT` (instruction debug locations come from `FUNC_CODE_DEBUG_LOC` records plus `DEBUG_LOC_AGAIN`), and metadata-as-value operands of `llvm.dbg.declare`/`llvm.dbg.value`.
- [ ] A small `DebugLocation { Line, Column, Scope }` API, plus `Scope.File` with `Directory` and `FileName`, replacing the `LLVMExtensions.GetDI*` helpers.

### Phase 4: differential tests against LLVMSharp (temporary)

Lives only in the test project, and is deleted in Phase 7.

- [ ] A text dumper over the new model and a second dumper over LLVMSharp, producing the **same format**: datalayout and triple; globals (linkage, constness, initializer rendered as a constant); functions (type, declaration or definition, parameter names); every basic block and instruction in order with opcode, result type, operands (by value number), predicates, flags we read, GEP source type, alloca type, call function type, indices, shuffle masks, `phi` incoming pairs and `switch` cases; constants rendered structurally (including constant expressions); the debug location and local-variable names for each instruction we use; use-list order; and `SizeOfTypeInBits`/`OffsetOfElement` for every type that appears.
- [ ] One test that runs both over every file in `build/cache/clang` (5,022 `.bc`) plus every hand-written file, and over both clang versions (Apple clang 21 and the one in `build/llvm`), and asserts that the dumps are identical. Report the first differing line with the file name.
- [ ] Add a few focused cases that the corpus lacks (wide integers, packed structs, vectors of `i1`, `atomicrmw`, `freeze`, `switch` with many cases, large constants) as small hand-written `.ll` files, converted to `.bc` by clang.
- [ ] Fix every difference at its cause in the reader; do not special-case the dumper.

### Phase 5: port IR2IL to the new model

Mechanical, in this order, each step compiling and tested:

- [ ] `TypeSystem` (type mapping, `GetSizeOfTypeInBits`, struct offsets via `DataLayout`).
- [ ] `ModuleCompiler`, `CompiledModule`, `GlobalsILEmitter`, `ILEmitter`.
- [ ] `FunctionILEmitter` (and `FunctionILEmitter.VectorConversions.cs`): each `case LLVMOpcode.X` becomes a case on the opcode or a type pattern, using the typed instruction's properties instead of `LLVMExtensions` helpers.
- [ ] `Intrinsics/*` (`IntrinsicFunctionCallContext` etc.) and the `LLVMIntrinsics`-named helper types stay, but take the new `Value`/`Type` classes.
- [ ] Remove `LLVMExtensions.cs` and `LLVMSourceCode.cs`; what is still useful moves onto the model classes.
- [ ] While the port is in progress, the Phase 4 differential test keeps running, so a reader fix cannot be broken by emitter work.

The suite (5,022 tests) must pass with the new reader as the only parser before the next phase.

### Phase 6: switch off LLVMSharp

- [ ] Remove `LLVMSharp.Interop` and the two `libLLVM.runtime.*` packages from `IR2IL.csproj`, and any `using LLVMSharp…` left.
- [ ] Check that nothing else (benchmarks project, tests) references them except the Phase 4 dumper.
- [ ] Run the full suite plus a manual `IR2IL in.bc out.exe` from a different working directory.
- [ ] Check startup time and per-test time against the baseline in `docs/TODO.md` (the native library load should no longer be part of it).

### Phase 7: delete the temporary scaffolding

- [ ] Delete the LLVMSharp-side dumper, the dump comparison test, and the LLVMSharp reference from `IR2IL.Tests`. Delete the new-model dumper too: it exists only for this comparison and is not maintained.
- [ ] Update `CLAUDE.md` (architecture section: the pipeline now reads bitcode; add a short "Adding an instruction / record / metadata kind" section), `README.md` (users pass `.bc`; show `clang -c -emit-llvm`), and `docs/TODO.md`.
- [ ] Mark this plan done.

## Extensibility: how new things get added

The aim is that supporting a new instruction, constant kind or metadata record is a few lines in one place:

- **Record dispatch by table.** `FunctionReader`, `ConstantsReader` and `MetadataReader` each map record code → handler method (`[RecordCode(FUNC_CODE_INST_CMPXCHG)]` or a dictionary). A new instruction is a new `Instruction` subclass, a new `Opcode` value and one handler.
- **Operand reading helpers** (`ReadRelativeValue`, `ReadTypedValue`, `ReadType`, `ReadAlignment`) are shared, so a handler is mostly a list of fields.
- **Fail loudly on unknown instructions and constants** (`NotSupportedException` with the record name and code); **skip silently, but keep an opaque node, for unknown metadata and blocks**, since the emitter does not need them.
- **Codes are enums** copied from `LLVMBitCodes.h` with the LLVM version noted, so a new LLVM release is a diff against that file.

## Risks and open questions

- **Bitcode version drift.** Tolerance is by design (trailing fields ignored), but newer writers could change a record form. The differential test over two clang versions is the check; new clang versions get added to that list.
- **No trusted dump tool locally** (`llvm-dis` and `llvm-bcanalyzer` are not installed). LLVMSharp is our oracle until Phase 6.
- **Use-list order.** `GetUses` order from LLVM depends on the order uses were created while parsing. Check how the emitter consumes it (single-use checks versus first user). If order matters, the reader must reproduce it; if only count and identity matter, the differential test compares them as sets.
- **`Int128`-and-wider constants.** Constants wider than 64 bits are stored as arrays of 64-bit words; the reader must sign-extend correctly for widths that are not multiples of 64 (the emitter parses the printed signed value today).
- **`unsafe` and pointer-conversion pitfalls disappear** with LLVMSharp (the old `CS9363` build break came from them), which also removes `LangVersion` workarounds.
- **Size of the port.** `FunctionILEmitter.cs` is 2,931 lines. Doing Phase 5 in the listed order, with tests after each step, keeps it reviewable; do not mix emitter refactoring into it.
- **Reader performance.** The corpus is 5,022 modules, most under 1 MB. A straightforward single-pass reader should beat the native-library startup, but this is measured in Phase 6, not assumed.
