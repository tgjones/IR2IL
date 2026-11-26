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
/// A vector reduction (<c>llvm.vector.reduce.add.v4i32</c>, <c>llvm.vector.reduce.fadd.v8f32</c>, …) of any shape,
/// resolved from its name by <see cref="TryCreate"/>. <c>add</c> (and a <c>reassoc</c> <c>fadd</c>) call the BCL
/// <c>Sum</c> of the container type; the other operations call <see cref="VectorReduction"/>. The padding lanes of a
/// padded vector (<c>&lt;3 x i32&gt;</c>) are first overwritten with the identity of the operation. A floating point
/// reduction without <c>reassoc</c> is ordered, so it is emitted as a chain of <c>GetElement</c> and <c>add</c>/<c>mul</c>.
/// </summary>
internal sealed class LLVMVectorReduceIntrinsicFunction(
    MethodInfo? method,
    Type nonGenericVectorType,
    Type vectorType,
    Type laneType,
    Type? unsignedLaneType,
    Type reducedVectorType,
    Type reducedLaneType,
    int laneCount,
    bool isPadded,
    object? identity,
    bool hasStart,
    OpCode? combine) : IntrinsicFunction
{
    // Intrinsic base name -> (read as signed, identity of the operation for a lane type; null for the sum, whose
    // identity is zero).
    private static readonly Dictionary<string, (bool Signed, Func<Type, object> Identity)> IntegerOperations = new()
    {
        ["add"] = (true, _ => 0),
        ["mul"] = (true, _ => 1),
        ["and"] = (true, _ => -1L),
        ["or"] = (true, _ => 0),
        ["xor"] = (true, _ => 0),
        ["smax"] = (true, lane => MinValue(lane)),
        ["smin"] = (true, lane => MaxValue(lane)),
        ["umax"] = (false, _ => 0),
        ["umin"] = (false, lane => MaxValue(lane)),
    };

    public override bool IsPure => true;

    private static object MinValue(Type lane) => Type.GetTypeCode(lane) switch
    {
        TypeCode.SByte => (long)sbyte.MinValue,
        TypeCode.Int16 => (long)short.MinValue,
        TypeCode.Int32 => (long)int.MinValue,
        _ => long.MinValue,
    };

    private static object MaxValue(Type lane) => Type.GetTypeCode(lane) switch
    {
        TypeCode.SByte => (long)sbyte.MaxValue,
        TypeCode.Byte => (long)byte.MaxValue,
        TypeCode.Int16 => (long)short.MaxValue,
        TypeCode.UInt16 => (long)ushort.MaxValue,
        TypeCode.Int32 => (long)int.MaxValue,
        TypeCode.UInt32 => (long)uint.MaxValue,
        TypeCode.Int64 => long.MaxValue,
        _ => ulong.MaxValue,
    };

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
        var nonGeneric = typeSystem.GetNonGenericVectorType(vectorType);
        var lane = typeSystem.GetMsilVectorElementType(element);
        var laneCount = (int)vectorType.VectorSize;
        var padded = typeSystem.IsPaddedVector(vectorType);

        if (IntegerOperations.TryGetValue(baseName, out var integer)
            && element.Kind == LLVMTypeKind.LLVMIntegerTypeKind && element.IntWidth is 8 or 16 or 32 or 64)
        {
            var reducedLane = integer.Signed ? lane : LaneTypes.GetUnsignedLaneType(lane);
            var reducedVector = vector.GetGenericTypeDefinition().MakeGenericType(reducedLane);
            var method = baseName == "add"
                ? VectorIntrinsicEmitter.GetVectorMethod(nonGeneric, "Sum", reducedVector, reducedLane, 1)
                : GetReduction(baseName, reducedVector, reducedLane);
            intrinsic = new LLVMVectorReduceIntrinsicFunction(
                method, nonGeneric, vector, lane, integer.Signed ? null : reducedLane, reducedVector, reducedLane,
                laneCount, padded, integer.Identity(reducedLane), hasStart: false, combine: null);
            return true;
        }

        if (baseName is "fadd" or "fmul"
            && element.Kind is LLVMTypeKind.LLVMFloatTypeKind or LLVMTypeKind.LLVMDoubleTypeKind)
        {
            var isAdd = baseName == "fadd";
            var combine = isAdd ? OpCodes.Add : OpCodes.Mul;
            MethodInfo? method = null;
            if (reassociate)
            {
                method = isAdd
                    ? VectorIntrinsicEmitter.GetVectorMethod(nonGeneric, "Sum", vector, lane, 1)
                    : GetReduction("mul", vector, lane);
            }

            intrinsic = new LLVMVectorReduceIntrinsicFunction(
                method, nonGeneric, vector, lane, null, vector, lane,
                laneCount, padded, isAdd ? -0.0 : 1.0, hasStart: true, combine);
            return true;
        }

        return false;
    }

    // The VectorReduction method for the container type, made for the lane type.
    private static MethodInfo GetReduction(string baseName, Type container, Type lane)
    {
        var methodName = baseName switch
        {
            "mul" => nameof(VectorReduction.Multiply),
            "and" => nameof(VectorReduction.And),
            "or" => nameof(VectorReduction.Or),
            "xor" => nameof(VectorReduction.Xor),
            "smax" or "umax" => nameof(VectorReduction.Max),
            _ => nameof(VectorReduction.Min),
        };
        var definition = container.GetGenericTypeDefinition();
        return typeof(VectorReduction).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == methodName && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == definition)
            .MakeGenericMethod(lane);
    }

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        // The operands are the optional start value, the vector and the callee.
        var il = context.ILGenerator;
        var vectorOperand = context.Operands[^2];
        if (hasStart)
        {
            context.EmitValue(context.Operands[0]);
        }

        context.EmitValue(vectorOperand);

        if (method == null)
        {
            // Ordered: combine the start value with each real lane in turn.
            var local = il.DeclareLocal(vectorType);
            il.Emit(OpCodes.Stloc, local);
            var getElement = nonGenericVectorType.GetStaticMethodStrict(nameof(System.Runtime.Intrinsics.Vector128.GetElement)).MakeGenericMethod(laneType);
            for (var i = 0; i < laneCount; i++)
            {
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Call, getElement);
                il.Emit(combine!.Value);
            }

            return;
        }

        if (unsignedLaneType != null)
        {
            VectorIntrinsicEmitter.EmitReinterpret(il, nonGenericVectorType, laneType, unsignedLaneType);
        }

        if (isPadded)
        {
            VectorIntrinsicEmitter.EmitFillPaddingLanes(il, nonGenericVectorType, reducedVectorType, reducedLaneType, laneCount, identity!);
        }

        il.Emit(OpCodes.Call, method);
        if (combine != null)
        {
            il.Emit(combine.Value);
        }
    }
}
