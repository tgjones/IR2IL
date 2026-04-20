using System;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;

namespace IR2IL.Intrinsics;

/// <summary>
/// Handles llvm.abs.i32 / llvm.abs.i64, which have the signature:
///   declare i32 @llvm.abs.i32(i32 %val, i1 immarg %is_int_min_poison)
/// The second operand is an immediate flag and must not be emitted as a stack value.
/// </summary>
internal sealed class LLVMAbsIntIntrinsicFunction(MethodInfo method) : IntrinsicFunction
{
    public static LLVMAbsIntIntrinsicFunction Create(Type type, params Type[] paramTypes)
        => new(type.GetMethodStrict(nameof(Math.Abs), paramTypes));

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
        context.ILGenerator.Emit(OpCodes.Call, method);
    }
}
