namespace IR2IL.Intrinsics;

internal sealed class LLVMThreadLocalAddressIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
    }
}