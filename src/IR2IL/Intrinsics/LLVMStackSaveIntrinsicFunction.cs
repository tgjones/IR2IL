using System.Reflection.Emit;
using IR2IL.Runtime;

namespace IR2IL.Intrinsics;

/// <summary><c>llvm.stacksave</c>: the top of the arena of dynamic allocas (<see cref="DynamicStack"/>).</summary>
internal sealed class LLVMStackSaveIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.ILGenerator.Emit(OpCodes.Call, typeof(DynamicStack).GetMethod(nameof(DynamicStack.Save))!);
    }
}
