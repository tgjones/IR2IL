namespace IR2IL.Intrinsics;

internal sealed class NoOpIntrinsicFunction : IntrinsicFunction
{
    public static readonly NoOpIntrinsicFunction Instance = new();

    public override bool IsPure => true;

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // Nothing to do.
    }
}
