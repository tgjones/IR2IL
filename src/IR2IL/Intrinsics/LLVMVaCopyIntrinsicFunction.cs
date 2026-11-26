using System.Reflection.Emit;

namespace IR2IL.Intrinsics;

/// <summary><c>llvm.va_copy</c>: copies the <c>va_list</c> object.</summary>
internal sealed class LLVMVaCopyIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // declare void @llvm.va_copy.p0(ptr <dest>, ptr <src>)
        context.EmitValue(context.Operands[0]);
        context.EmitValue(context.Operands[1]);
        context.ILGenerator.Emit(OpCodes.Ldc_I4, context.TypeSystem.VaListLayout.SizeInBytes);
        context.ILGenerator.Emit(OpCodes.Cpblk);
    }
}
