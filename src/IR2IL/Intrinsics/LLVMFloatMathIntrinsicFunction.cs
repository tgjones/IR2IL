using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// A floating point math intrinsic (<c>llvm.sin.f32</c>, <c>llvm.sin.v8f32</c>) resolved from its name by
/// <see cref="TryCreate"/>, for scalars and vectors of any shape. Scalars call <c>MathF</c> or <c>Math</c>. Vectors call
/// the BCL method of the vector type when there is one, and otherwise apply the scalar method to each lane.
/// A scalar <c>half</c> (<c>llvm.sin.f16</c>) calls the static method of the same name on <see cref="Half"/>;
/// there are no vectors of <c>half</c>.
/// </summary>
internal sealed class LLVMFloatMathIntrinsicFunction(
    MethodInfo method,
    int arity,
    Type? nonGenericVectorType,
    Type? vectorType,
    Type laneType,
    int laneCount,
    bool perLane,
    int extraArgument = -1) : IntrinsicFunction
{
    // Intrinsic base name -> (.NET name of the scalar and vector methods, number of operands, whether the vector types have a method).
    private static readonly Dictionary<string, (string Name, int Arity, bool HasVectorMethod)> Operations = new()
    {
        ["sin"] = ("Sin", 1, true), ["cos"] = ("Cos", 1, true), ["asin"] = ("Asin", 1, true),
        ["exp"] = ("Exp", 1, true), ["log"] = ("Log", 1, true), ["sqrt"] = ("Sqrt", 1, true),
        ["fabs"] = ("Abs", 1, true), ["ceil"] = ("Ceiling", 1, true), ["copysign"] = ("CopySign", 2, true),
        ["fmuladd"] = ("FusedMultiplyAdd", 3, true),
        ["tan"] = ("Tan", 1, false), ["acos"] = ("Acos", 1, false), ["atan"] = ("Atan", 1, false),
        ["sinh"] = ("Sinh", 1, false), ["cosh"] = ("Cosh", 1, false), ["tanh"] = ("Tanh", 1, false),
        ["log10"] = ("Log10", 1, false), ["atan2"] = ("Atan2", 2, false), ["pow"] = ("Pow", 2, false),
    };

    // Intrinsic base name -> (name of the Half method, number of operands, and the MidpointRounding to pass, or -1).
    // Half computes in float and rounds once, as clang does for these (it calls the float libm function).
    private static readonly Dictionary<string, (string Name, int Arity, int Extra)> HalfOperations = new()
    {
        ["sqrt"] = ("Sqrt", 1, -1), ["fabs"] = ("Abs", 1, -1), ["ceil"] = ("Ceiling", 1, -1), ["floor"] = ("Floor", 1, -1),
        ["trunc"] = ("Truncate", 1, -1), ["round"] = ("Round", 1, (int)MidpointRounding.AwayFromZero),
        ["roundeven"] = ("Round", 1, -1), ["rint"] = ("Round", 1, -1), ["nearbyint"] = ("Round", 1, -1),
        ["copysign"] = ("CopySign", 2, -1), ["fma"] = ("FusedMultiplyAdd", 3, -1), ["fmuladd"] = ("FusedMultiplyAdd", 3, -1),
        ["minnum"] = ("MinNumber", 2, -1), ["maxnum"] = ("MaxNumber", 2, -1),
        ["pow"] = ("Pow", 2, -1), ["atan2"] = ("Atan2", 2, -1),
        ["exp"] = ("Exp", 1, -1), ["exp2"] = ("Exp2", 1, -1), ["log"] = ("Log", 1, -1), ["log2"] = ("Log2", 1, -1),
        ["log10"] = ("Log10", 1, -1), ["sin"] = ("Sin", 1, -1), ["cos"] = ("Cos", 1, -1), ["tan"] = ("Tan", 1, -1),
        ["asin"] = ("Asin", 1, -1), ["acos"] = ("Acos", 1, -1), ["atan"] = ("Atan", 1, -1),
        ["sinh"] = ("Sinh", 1, -1), ["cosh"] = ("Cosh", 1, -1), ["tanh"] = ("Tanh", 1, -1),
    };

    public override bool IsPure => true;

    /// <summary>Resolves <c>llvm.&lt;operation&gt;.&lt;f32|f64|v&lt;N&gt;f32|v&lt;N&gt;f64&gt;</c>, or returns <see langword="false"/>.</summary>
    public static bool TryCreate(string name, LLVMTypeRef resultType, TypeSystem typeSystem, out IntrinsicFunction? intrinsic)
    {
        intrinsic = null;
        var parts = name.Split('.');
        if (parts.Length != 3 || parts[0] != "llvm")
        {
            return false;
        }

        if (resultType.Kind == LLVMTypeKind.LLVMHalfTypeKind && parts[2] == "f16")
        {
            if (!HalfOperations.TryGetValue(parts[1], out var halfOperation))
            {
                return false;
            }

            var halfParameters = Enumerable.Repeat(typeof(Half), halfOperation.Arity)
                .Concat(halfOperation.Extra >= 0 ? [typeof(MidpointRounding)] : []).ToArray();
            intrinsic = new LLVMFloatMathIntrinsicFunction(
                typeof(Half).GetMethodStrict(halfOperation.Name, halfParameters),
                halfOperation.Arity, null, null, typeof(Half), 0, perLane: false, halfOperation.Extra);
            return true;
        }

        if (!Operations.TryGetValue(parts[1], out var operation))
        {
            return false;
        }

        var isVector = resultType.Kind == LLVMTypeKind.LLVMVectorTypeKind;
        var elementType = isVector ? resultType.ElementType : resultType;
        if (elementType.Kind is not (LLVMTypeKind.LLVMFloatTypeKind or LLVMTypeKind.LLVMDoubleTypeKind))
        {
            return false;
        }

        var isFloat = elementType.Kind == LLVMTypeKind.LLVMFloatTypeKind;
        var laneType = isFloat ? typeof(float) : typeof(double);
        var scalar = (isFloat ? typeof(MathF) : typeof(Math))
            .GetMethodStrict(operation.Name, Enumerable.Repeat(laneType, operation.Arity).ToArray());

        if (!isVector)
        {
            intrinsic = new LLVMFloatMathIntrinsicFunction(scalar, operation.Arity, null, null, laneType, 0, perLane: false);
            return true;
        }

        var vectorType = typeSystem.GetMsilVectorType(resultType);
        var nonGenericVectorType = typeSystem.GetNonGenericVectorType(resultType);
        var laneCount = (int)resultType.VectorSize;
        intrinsic = operation.HasVectorMethod
            ? new LLVMFloatMathIntrinsicFunction(
                VectorIntrinsicEmitter.GetVectorMethod(nonGenericVectorType, operation.Name, vectorType, laneType, operation.Arity),
                operation.Arity, nonGenericVectorType, vectorType, laneType, laneCount, perLane: false)
            : new LLVMFloatMathIntrinsicFunction(scalar, operation.Arity, nonGenericVectorType, vectorType, laneType, laneCount, perLane: true);
        return true;
    }

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        for (var i = 0; i < arity; i++)
        {
            context.EmitValue(context.Operands[i]);
        }

        if (extraArgument >= 0)
        {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, extraArgument);
        }

        if (perLane)
        {
            VectorIntrinsicEmitter.EmitPerLane(context.ILGenerator, nonGenericVectorType!, vectorType!, laneType, laneCount, arity, method);
        }
        else
        {
            context.ILGenerator.Emit(OpCodes.Call, method);
        }
    }
}
