using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// <c>llvm.experimental.cttz.elts.iN.vMi1</c> of any mask shape, resolved from its name by <see cref="TryCreate"/>. It
/// calls <see cref="LLVMIntrinsics.CttzElems(System.Runtime.Intrinsics.Vector64{sbyte}, int)"/> of the container of the
/// mask with the number of real lanes, and ignores the poison-on-zero flag (a mask with no true lane gives the lane count either way).
/// </summary>
internal sealed class LLVMCttzElemsIntrinsicFunction(MethodInfo method, int laneCount) : IntrinsicFunction
{
    public override bool IsPure => true;

    public static bool TryCreate(string name, LLVMTypeRef maskType, TypeSystem typeSystem, out IntrinsicFunction? intrinsic)
    {
        intrinsic = null;
        if (!name.StartsWith("llvm.experimental.cttz.elts.", StringComparison.Ordinal)
            || maskType.Kind != LLVMTypeKind.LLVMVectorTypeKind
            || maskType.ElementType.Kind != LLVMTypeKind.LLVMIntegerTypeKind
            || maskType.ElementType.IntWidth != 1)
        {
            return false;
        }

        var container = typeSystem.GetMsilVectorType(maskType);
        var method = typeof(LLVMIntrinsics).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == nameof(LLVMIntrinsics.CttzElems) && m.GetParameters()[0].ParameterType == container)
            ?? throw new NotSupportedException($"{name}: no implementation for a mask held in {container}.");
        intrinsic = new LLVMCttzElemsIntrinsicFunction(method, (int)maskType.VectorSize);
        return true;
    }

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
        context.ILGenerator.Emit(OpCodes.Ldc_I4, laneCount);
        context.ILGenerator.Emit(OpCodes.Call, method);
        if (context.Callee.ReturnType == typeof(long))
        {
            context.ILGenerator.Emit(OpCodes.Conv_I8);
        }
    }
}
