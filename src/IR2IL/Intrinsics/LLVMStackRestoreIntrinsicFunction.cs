using System.Reflection.Emit;
using IR2IL.Runtime;

namespace IR2IL.Intrinsics;

/// <summary><c>llvm.stackrestore</c>: releases the dynamic allocas made since the matching <c>llvm.stacksave</c>.</summary>
internal sealed class LLVMStackRestoreIntrinsicFunction : IntrinsicFunction
{
    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
        context.ILGenerator.Emit(OpCodes.Call, typeof(DynamicStack).GetMethod(nameof(DynamicStack.Restore))!);
    }
}
