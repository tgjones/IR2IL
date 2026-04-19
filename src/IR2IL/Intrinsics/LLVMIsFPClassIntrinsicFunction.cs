using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;

namespace IR2IL.Intrinsics;

internal sealed class LLVMIsFPClassIntrinsicFunction(bool isDouble) : IntrinsicFunction
{
    private static readonly MethodInfo MethodF32 = typeof(LLVMIntrinsics).GetMethodStrict(nameof(LLVMIntrinsics.IsFPClassF32), [typeof(float), typeof(int)]);
    private static readonly MethodInfo MethodF64 = typeof(LLVMIntrinsics).GetMethodStrict(nameof(LLVMIntrinsics.IsFPClassF64), [typeof(double), typeof(int)]);

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // declare i1 @llvm.is.fpclass.f64(double %val, i32 immarg %mask)
        // declare i1 @llvm.is.fpclass.f32(float %val, i32 immarg %mask)
        context.EmitValue(context.Operands[0]);
        context.ILGenerator.Emit(OpCodes.Ldc_I4, (int)context.Operands[1].ConstIntZExt);
        context.ILGenerator.Emit(OpCodes.Call, isDouble ? MethodF64 : MethodF32);
    }
}
