namespace IR2IL.Intrinsics;

/// <summary>
/// <c>llvm.expect.iN(value, expected)</c>: a hint for branch layout. The result is the value; the expected value
/// is not evaluated.
/// </summary>
internal sealed class LLVMExpectIntrinsicFunction : IntrinsicFunction
{
    public override bool IsPure => true;

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
    }
}
