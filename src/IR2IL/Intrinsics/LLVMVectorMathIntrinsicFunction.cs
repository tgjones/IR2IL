using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// A floating point math intrinsic on a vector of any shape (<c>llvm.sin.v8f32</c>), resolved from its name by
/// <see cref="TryCreate"/> when the shape has no entry of its own in <see cref="IntrinsicFunctions.LLVMIntrinsics"/>.
/// It calls the generic <see cref="VectorMath"/> method for the arity, which applies the scalar operation to each lane.
/// </summary>
internal sealed class LLVMVectorMathIntrinsicFunction(System.Reflection.MethodInfo method, int laneCount, int arity) : IntrinsicFunction
{
    // Intrinsic base name -> (operation type, number of operands).
    private static readonly Dictionary<string, (Type Operation, int Arity)> Operations = new()
    {
        ["sin"] = (typeof(VectorMath.Sin<>), 1),
        ["cos"] = (typeof(VectorMath.Cos<>), 1),
        ["tan"] = (typeof(VectorMath.Tan<>), 1),
        ["asin"] = (typeof(VectorMath.Asin<>), 1),
        ["acos"] = (typeof(VectorMath.Acos<>), 1),
        ["atan"] = (typeof(VectorMath.Atan<>), 1),
        ["sinh"] = (typeof(VectorMath.Sinh<>), 1),
        ["cosh"] = (typeof(VectorMath.Cosh<>), 1),
        ["tanh"] = (typeof(VectorMath.Tanh<>), 1),
        ["exp"] = (typeof(VectorMath.Exp<>), 1),
        ["log"] = (typeof(VectorMath.Log<>), 1),
        ["log10"] = (typeof(VectorMath.Log10<>), 1),
        ["sqrt"] = (typeof(VectorMath.Sqrt<>), 1),
        ["fabs"] = (typeof(VectorMath.Fabs<>), 1),
        ["ceil"] = (typeof(VectorMath.Ceil<>), 1),
        ["atan2"] = (typeof(VectorMath.Atan2<>), 2),
        ["pow"] = (typeof(VectorMath.Pow<>), 2),
        ["copysign"] = (typeof(VectorMath.CopySign<>), 2),
        ["fmuladd"] = (typeof(VectorMath.FusedMultiplyAdd<>), 3),
    };

    public override bool IsPure => true;

    /// <summary>Resolves <c>llvm.&lt;operation&gt;.v&lt;N&gt;f32|f64</c>, or returns <see langword="false"/>.</summary>
    public static bool TryCreate(string name, LLVMTypeRef resultType, TypeSystem typeSystem, out IntrinsicFunction? intrinsic)
    {
        intrinsic = null;
        var parts = name.Split('.');
        if (parts.Length != 3 || parts[0] != "llvm" || !Operations.TryGetValue(parts[1], out var operation)
            || resultType.Kind != LLVMTypeKind.LLVMVectorTypeKind
            || resultType.ElementType.Kind is not (LLVMTypeKind.LLVMFloatTypeKind or LLVMTypeKind.LLVMDoubleTypeKind))
        {
            return false;
        }

        var laneType = typeSystem.GetMsilVectorElementType(resultType.ElementType);
        var vectorType = typeSystem.GetMsilVectorType(resultType);
        var methodName = operation.Arity switch { 1 => nameof(VectorMath.Unary), 2 => nameof(VectorMath.Binary), _ => nameof(VectorMath.Ternary) };
        var method = typeof(VectorMath).GetStaticMethodStrict(methodName)
            .MakeGenericMethod(vectorType, laneType, operation.Operation.MakeGenericType(laneType));
        intrinsic = new LLVMVectorMathIntrinsicFunction(method, (int)resultType.VectorSize, operation.Arity);
        return true;
    }

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        for (var i = 0; i < arity; i++)
        {
            context.EmitValue(context.Operands[i]);
        }

        context.ILGenerator.Emit(OpCodes.Ldc_I4, laneCount);
        context.ILGenerator.Emit(OpCodes.Call, method);
    }
}
