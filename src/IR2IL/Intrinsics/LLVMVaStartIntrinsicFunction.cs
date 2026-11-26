using System.Reflection.Emit;

namespace IR2IL.Intrinsics;

/// <summary>
/// <c>llvm.va_start</c>: a variadic function compiles to a method with a hidden last <c>void*</c> parameter,
/// the address of the buffer of 8-byte slots that holds the extra arguments of the call.
/// </summary>
internal sealed class LLVMVaStartIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        var il = context.ILGenerator;
        var vaBufferArgumentIndex = context.VaBufferArgumentIndex;

        context.EmitValue(context.Operands[0]);
        context.TypeSystem.VaListLayout.EmitVaStart(
            il,
            () => il.Emit(OpCodes.Ldarg, vaBufferArgumentIndex));
    }
}
