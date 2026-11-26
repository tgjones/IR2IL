using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// An integer intrinsic (<c>abs</c>, <c>smin</c>, <c>smax</c>, <c>umin</c>, <c>umax</c>, <c>scmp</c>, <c>ucmp</c>,
/// <c>ctpop</c>, <c>usub.sat</c>) on a scalar of up to 64 bits or a vector, resolved from the operand type by
/// <see cref="TryCreate"/> for every overload. A scalar operand is extended from its logical width as the intrinsic
/// reads it (signed or unsigned) and passed to a <c>Math</c>, <c>uint</c>/<c>ulong</c> or <see cref="LLVMIntrinsics"/>
/// method. A vector calls the BCL method of its container type directly (unsigned operations reinterpret the lanes
/// first), or <see cref="VectorUtility"/>.PopCount.
/// </summary>
internal sealed class LLVMIntegerIntrinsicFunction(
    MethodInfo method,
    int operandCount,
    bool isSigned,
    int resultBits,
    Type? nonGenericVectorType = null,
    Type? laneType = null,
    Type? unsignedLaneType = null) : IntrinsicFunction
{
    // Intrinsic base name -> (number of value operands, read as signed, name of the vector method; null for ctpop, which
    // uses VectorUtility.PopCount, and for scmp and ucmp, which have no vector form).
    private static readonly Dictionary<string, (int Arity, bool Signed, string? VectorMethod)> Operations = new()
    {
        ["abs"] = (1, true, "Abs"),
        ["smin"] = (2, true, "Min"),
        ["smax"] = (2, true, "Max"),
        ["umin"] = (2, false, "Min"),
        ["umax"] = (2, false, "Max"),
        ["usub.sat"] = (2, false, "SubtractSaturate"),
        ["ctpop"] = (1, false, null),
        ["scmp"] = (2, true, null),
        ["ucmp"] = (2, false, null),
    };

    public override bool IsPure => true;

    /// <summary>Resolves an integer intrinsic of the type of its first operand, or returns <see langword="false"/>.</summary>
    public static bool TryCreate(string name, LLVMTypeRef operandType, LLVMTypeRef resultType, TypeSystem typeSystem, out IntrinsicFunction? intrinsic)
    {
        intrinsic = null;
        var baseName = FindBaseName(name);
        if (baseName == null)
        {
            return false;
        }

        var (arity, isSigned, vectorMethodName) = Operations[baseName];
        if (operandType.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            if (baseName is "scmp" or "ucmp"
                || operandType.ElementType.Kind != LLVMTypeKind.LLVMIntegerTypeKind
                || operandType.ElementType.IntWidth is not (8 or 16 or 32 or 64))
            {
                return false;
            }

            var lane = typeSystem.GetMsilVectorElementType(operandType.ElementType);
            var vectorType = typeSystem.GetMsilVectorType(operandType);
            var nonGeneric = typeSystem.GetNonGenericVectorType(operandType);
            MethodInfo vectorMethod;
            Type? unsignedLane = null;
            if (vectorMethodName == null)
            {
                var definition = vectorType.GetGenericTypeDefinition();
                vectorMethod = typeof(VectorUtility).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(m => m.Name == nameof(VectorUtility.PopCount)
                        && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == definition)
                    .MakeGenericMethod(lane);
            }
            else if (isSigned)
            {
                vectorMethod = VectorIntrinsicEmitter.GetVectorMethod(nonGeneric, vectorMethodName, vectorType, lane, arity);
            }
            else
            {
                unsignedLane = LaneTypes.GetUnsignedLaneType(lane);
                var unsignedVector = vectorType.GetGenericTypeDefinition().MakeGenericType(unsignedLane);
                vectorMethod = VectorIntrinsicEmitter.GetVectorMethod(nonGeneric, vectorMethodName, unsignedVector, unsignedLane, arity);
            }

            intrinsic = new LLVMIntegerIntrinsicFunction(vectorMethod, arity, isSigned, 0, nonGeneric, lane, unsignedLane);
            return true;
        }

        if (operandType.Kind != LLVMTypeKind.LLVMIntegerTypeKind || operandType.IntWidth > 64)
        {
            return false;
        }

        var is64 = operandType.IntWidth > 32;
        var scalar = is64 ? (isSigned ? typeof(long) : typeof(ulong)) : (isSigned ? typeof(int) : typeof(uint));
        var parameters = Enumerable.Repeat(scalar, arity).ToArray();
        var scalarMethod = baseName switch
        {
            "abs" => typeof(LLVMIntrinsics).GetMethodStrict(nameof(LLVMIntrinsics.Abs), parameters),
            "smin" or "umin" => typeof(Math).GetMethodStrict(nameof(Math.Min), parameters),
            "smax" or "umax" => typeof(Math).GetMethodStrict(nameof(Math.Max), parameters),
            "ctpop" => scalar.GetMethodStrict("PopCount", parameters),
            "usub.sat" => typeof(LLVMIntrinsics).GetMethodStrict(nameof(LLVMIntrinsics.UnsignedSubtractSaturate), parameters),
            _ => typeof(LLVMIntrinsics).GetStaticMethodStrict(nameof(LLVMIntrinsics.Compare)).MakeGenericMethod(scalar),
        };
        intrinsic = new LLVMIntegerIntrinsicFunction(scalarMethod, arity, isSigned, baseName is "scmp" or "ucmp" ? (int)resultType.IntWidth : 0);
        return true;
    }

    // `llvm.<base name>.<overloaded types>`: the base name may itself have a dot (`usub.sat`).
    private static string? FindBaseName(string name)
    {
        foreach (var baseName in Operations.Keys)
        {
            if (name.StartsWith("llvm." + baseName + ".", StringComparison.Ordinal))
            {
                return baseName;
            }
        }

        return null;
    }

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        var il = context.ILGenerator;

        // declare i32 @llvm.abs.i32(i32 %val, i1 immarg %is_int_min_poison): the flag is not a stack value.
        for (var i = 0; i < operandCount; i++)
        {
            if (nonGenericVectorType != null)
            {
                context.EmitValue(context.Operands[i]);
                if (unsignedLaneType != null)
                {
                    VectorIntrinsicEmitter.EmitReinterpret(il, nonGenericVectorType, laneType!, unsignedLaneType);
                }
            }
            else if (isSigned)
            {
                context.EmitSignedInteger(context.Operands[i]);
            }
            else
            {
                context.EmitUnsignedInteger(context.Operands[i]);
            }
        }

        il.Emit(OpCodes.Call, method);

        if (unsignedLaneType != null)
        {
            VectorIntrinsicEmitter.EmitReinterpret(il, nonGenericVectorType!, unsignedLaneType, laneType!);
        }

        // A comparison returns an int; the result type may be an i64.
        if (resultBits > 32)
        {
            il.Emit(OpCodes.Conv_I8);
        }
    }
}
