using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

internal readonly ref struct IntrinsicFunctionCallContext(
    MethodInfo callee,
    IReadOnlyDictionary<LLVMValueRef, LocalBuilder> locals,
    ILGenerator ilGenerator,
    LLVMValueRef[] operands,
    Action<LLVMValueRef> emitValue,
    Action<LLVMValueRef> emitSignedInteger,
    Action<LLVMValueRef> emitUnsignedInteger,
    TypeSystem typeSystem)
{
    public MethodInfo Callee { get; } = callee;
    public IReadOnlyDictionary<LLVMValueRef, LocalBuilder> Locals { get; } = locals;
    public ILGenerator ILGenerator { get; } = ilGenerator;
    public LLVMValueRef[] Operands { get; } = operands;
    public Action<LLVMValueRef> EmitValue { get; } = emitValue;
    public TypeSystem TypeSystem { get; } = typeSystem;

    /// <summary>Pushes an integer operand sign-extended from its logical width (see CLAUDE.md, "Normalisation").</summary>
    public Action<LLVMValueRef> EmitSignedInteger { get; } = emitSignedInteger;

    /// <summary>Pushes an integer operand zero-extended from its logical width.</summary>
    public Action<LLVMValueRef> EmitUnsignedInteger { get; } = emitUnsignedInteger;
}
