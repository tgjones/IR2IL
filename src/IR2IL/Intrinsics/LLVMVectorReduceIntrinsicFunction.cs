using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// A vector reduction (<c>llvm.vector.reduce.add.v4i32</c>, <c>llvm.vector.reduce.fadd.v8f32</c>, …) of any shape,
/// resolved from its name by <see cref="TryCreate"/>. It calls <see cref="VectorMath.Reduce{TVector, TLane, TOperation}"/>
/// (for the floating point ones, which take a start value, <see cref="VectorMath.ReduceReassociated{TVector, TLane, TOperation}"/>
/// with the <c>reassoc</c> flag and <see cref="VectorMath.ReduceOrdered{TVector, TLane, TOperation}"/> without it).
/// The first two use SIMD operations; the ordered one is a loop over the real lanes.
/// </summary>
internal sealed class LLVMVectorReduceIntrinsicFunction(MethodInfo method, int laneCount) : IntrinsicFunction
{
    // Intrinsic base name -> (operation type, read as signed). `Max`, `Min` and `Add` serve both signednesses
    // through the lane type.
    private static readonly Dictionary<string, (Type Operation, bool Signed)> IntegerOperations = new()
    {
        ["add"] = (typeof(VectorMath.Add<>), true),
        ["mul"] = (typeof(VectorMath.Multiply<>), true),
        ["and"] = (typeof(VectorMath.BitAnd<>), true),
        ["or"] = (typeof(VectorMath.BitOr<>), true),
        ["xor"] = (typeof(VectorMath.BitXor<>), true),
        ["smax"] = (typeof(VectorMath.IntegerMax<>), true),
        ["smin"] = (typeof(VectorMath.IntegerMin<>), true),
        ["umax"] = (typeof(VectorMath.IntegerMax<>), false),
        ["umin"] = (typeof(VectorMath.IntegerMin<>), false),
    };

    private static readonly Dictionary<string, Type> FloatOperations = new()
    {
        ["fadd"] = typeof(VectorMath.Add<>),
        ["fmul"] = typeof(VectorMath.Multiply<>),
    };

    public override bool IsPure => true;

    /// <summary>Resolves <c>llvm.vector.reduce.&lt;operation&gt;.v&lt;N&gt;&lt;type&gt;</c>, or returns <see langword="false"/>.</summary>
    /// <param name="vectorType">The type of the vector operand (the last operand).</param>
    /// <param name="reassociate">Whether the call has the <c>reassoc</c> fast-math flag, so a floating point reduction may combine the lanes in any order.</param>
    public static bool TryCreate(string name, LLVMTypeRef vectorType, bool reassociate, TypeSystem typeSystem, out IntrinsicFunction? intrinsic)
    {
        intrinsic = null;
        const string prefix = "llvm.vector.reduce.";
        if (!name.StartsWith(prefix, StringComparison.Ordinal) || vectorType.Kind != LLVMTypeKind.LLVMVectorTypeKind)
        {
            return false;
        }

        var baseName = name[prefix.Length..].Split('.')[0];
        var element = vectorType.ElementType;
        var vector = typeSystem.GetMsilVectorType(vectorType);
        var lane = typeSystem.GetMsilVectorElementType(element);
        MethodInfo method;
        if (IntegerOperations.TryGetValue(baseName, out var integer)
            && element.Kind == LLVMTypeKind.LLVMIntegerTypeKind && element.IntWidth is 8 or 16 or 32 or 64)
        {
            lane = integer.Signed ? lane : ToUnsigned(lane);
            method = typeof(VectorMath).GetStaticMethodStrict(nameof(VectorMath.Reduce))
                .MakeGenericMethod(vector, lane, integer.Operation.MakeGenericType(lane));
        }
        else if (FloatOperations.TryGetValue(baseName, out var floating)
            && element.Kind is LLVMTypeKind.LLVMFloatTypeKind or LLVMTypeKind.LLVMDoubleTypeKind)
        {
            method = typeof(VectorMath).GetStaticMethodStrict(reassociate ? nameof(VectorMath.ReduceReassociated) : nameof(VectorMath.ReduceOrdered))
                .MakeGenericMethod(vector, lane, floating.MakeGenericType(lane));
        }
        else
        {
            return false;
        }

        intrinsic = new LLVMVectorReduceIntrinsicFunction(method, (int)vectorType.VectorSize);
        return true;
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
        // The last operand is the callee.
        foreach (var operand in context.Operands[..^1])
        {
            context.EmitValue(operand);
        }

        context.ILGenerator.Emit(OpCodes.Ldc_I4, laneCount);
        context.ILGenerator.Emit(OpCodes.Call, method);
    }
}
