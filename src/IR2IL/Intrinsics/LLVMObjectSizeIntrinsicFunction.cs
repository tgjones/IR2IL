using System.Reflection.Emit;

namespace IR2IL.Intrinsics;

internal sealed class LLVMObjectSizeIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // declare i64 @llvm.objectsize.i64.p0(ptr %ptr, i1 %min, i1 %nullunknown, i1 %dynamic)
        //
        // This intrinsic is evaluated at compile time. When the object size is statically
        // unknown (which is always the case at O0), it returns -1 if min=false, or 0 if min=true.
        var min = context.Operands[1].ConstIntZExt != 0;
        context.ILGenerator.Emit(OpCodes.Ldc_I8, min ? 0L : -1L);
    }
}
