using System;
using System.Collections.Generic;
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
/// reads it (signed or unsigned) and passed to the operation of <see cref="VectorMath"/>; a vector goes through the
/// lane loop of <see cref="VectorMath"/>.
/// </summary>
internal sealed class LLVMIntegerIntrinsicFunction(MethodInfo method, int operandCount, bool isSigned, int laneCount, int resultBits) : IntrinsicFunction
{
    // Intrinsic base name -> (operation type, number of value operands, read as signed).
    private static readonly Dictionary<string, (Type? Operation, int Arity, bool Signed)> Operations = new()
    {
        ["abs"] = (typeof(VectorMath.IntegerAbs<>), 1, true),
        ["smin"] = (typeof(VectorMath.IntegerMin<>), 2, true),
        ["smax"] = (typeof(VectorMath.IntegerMax<>), 2, true),
        ["umin"] = (typeof(VectorMath.IntegerMin<>), 2, false),
        ["umax"] = (typeof(VectorMath.IntegerMax<>), 2, false),
        ["ctpop"] = (typeof(VectorMath.PopCount<>), 1, false),
        ["usub.sat"] = (typeof(VectorMath.UnsignedSubtractSaturate<>), 2, false),
        // The result of a comparison has its own width, so it has no lane operation and no vector form.
        ["scmp"] = (null, 2, true),
        ["ucmp"] = (null, 2, false),
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

        var (operation, arity, isSigned) = Operations[baseName];
        if (operandType.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            if (operation == null
                || operandType.ElementType.Kind != LLVMTypeKind.LLVMIntegerTypeKind
                || operandType.ElementType.IntWidth is not (8 or 16 or 32 or 64))
            {
                return false;
            }

            var lane = typeSystem.GetMsilVectorElementType(operandType.ElementType);
            lane = isSigned ? lane : ToUnsigned(lane);
            var methodName = arity == 1 ? nameof(VectorMath.Unary) : nameof(VectorMath.Binary);
            var vectorMethod = typeof(VectorMath).GetStaticMethodStrict(methodName)
                .MakeGenericMethod(typeSystem.GetMsilVectorType(operandType), lane, operation.MakeGenericType(lane));
            intrinsic = new LLVMIntegerIntrinsicFunction(vectorMethod, arity, isSigned, (int)operandType.VectorSize, 0);
            return true;
        }

        if (operandType.Kind != LLVMTypeKind.LLVMIntegerTypeKind || operandType.IntWidth > 64)
        {
            return false;
        }

        var scalar = operandType.IntWidth <= 32 ? (isSigned ? typeof(int) : typeof(uint)) : (isSigned ? typeof(long) : typeof(ulong));
        var scalarMethod = operation == null
            ? typeof(VectorMath).GetStaticMethodStrict(nameof(VectorMath.Compare)).MakeGenericMethod(scalar)
            : operation.MakeGenericType(scalar).GetStaticMethodStrict("Apply");
        intrinsic = new LLVMIntegerIntrinsicFunction(scalarMethod, arity, isSigned, 0, operation == null ? (int)resultType.IntWidth : 0);
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

    private static Type ToUnsigned(Type lane) => Type.GetTypeCode(lane) switch
    {
        TypeCode.SByte => typeof(byte),
        TypeCode.Int16 => typeof(ushort),
        TypeCode.Int32 => typeof(uint),
        TypeCode.Int64 => typeof(ulong),
        _ => lane,
    };

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // declare i32 @llvm.abs.i32(i32 %val, i1 immarg %is_int_min_poison): the flag is not a stack value.
        for (var i = 0; i < operandCount; i++)
        {
            if (laneCount > 0)
            {
                context.EmitValue(context.Operands[i]);
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

        if (laneCount > 0)
        {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, laneCount);
        }

        context.ILGenerator.Emit(OpCodes.Call, method);

        // A comparison returns an int; the result type may be an i64.
        if (resultBits > 32)
        {
            context.ILGenerator.Emit(OpCodes.Conv_I8);
        }
    }
}
