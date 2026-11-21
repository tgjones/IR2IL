namespace IR2IL.Intrinsics;

public abstract class IntrinsicFunction
{
    /// <summary>
    /// Whether the intrinsic neither reads nor writes memory and has no other side effects, so a call to it
    /// can be evaluated later than where it appears. Defaults to <see langword="false"/>, which is always safe.
    /// </summary>
    public virtual bool IsPure => false;

    public abstract void BuildCall(IntrinsicFunctionCallContext context);
}
