using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.ILEmission;

/// <summary>
/// Vector conversions (<c>sext</c>, <c>zext</c>, <c>trunc</c>, <c>fpext</c>, <c>fptrunc</c>, <c>fptosi</c>,
/// <c>fptoui</c>, <c>sitofp</c>, <c>uitofp</c>), composed from a few BCL operations that are each one or two
/// instructions, so that no conversion needs a method of its own per pair of types:
/// <list type="bullet">
/// <item>a step that doubles or halves the lane width, keeping the lane count: <c>Create(WidenLower(v), WidenUpper(v))</c>
/// and <c>Narrow(GetLower(v), GetUpper(v))</c>;</item>
/// <item>a conversion of the kind of the lanes at one lane width: <c>ConvertToSingle</c>, <c>ConvertToInt64</c>, …;</item>
/// <item>a reinterpretation between the signed and unsigned lane type of the same width: <c>As</c>.</item>
/// </list>
/// Everything is emitted inline, so the JIT sees the intrinsics themselves.
/// </summary>
internal sealed partial class FunctionILEmitter
{
    /// <summary>
    /// Converts the vector of <paramref name="fromType"/> on the stack to <paramref name="toType"/> (same lane
    /// count). The lanes are converted at the wider of the two lane widths, as that is where the BCL has the
    /// operation: the source is widened first when it is narrower, and the result is narrowed afterwards when
    /// the destination is narrower. An integer is widened as signed or unsigned according to the opcode.
    /// <list type="bullet">
    /// <item><c>sext</c>, <c>zext</c>, <c>fpext</c>: widen.</item>
    /// <item><c>trunc</c>, <c>fptrunc</c>: narrow.</item>
    /// <item><c>sitofp</c>, <c>uitofp</c>, <c>fptosi</c>, <c>fptoui</c>: widen the source to the wider lane width,
    /// convert the kind there (<c>ConvertToDouble</c> of an <c>i16</c> lane widened to <c>i64</c>), narrow the result.</item>
    /// </list>
    /// <c>i64</c> lanes to <c>float</c> are converted one lane at a time, because the BCL has no vector conversion for
    /// them and going through <c>double</c> would round twice.
    /// </summary>
    private void EmitVectorLaneConversion(LLVMOpcode opcode, LLVMTypeRef fromType, LLVMTypeRef toType)
    {
        var laneCount = (int)fromType.VectorSize;
        var sourceLane = TypeSystem.GetMsilVectorElementType(fromType.ElementType);
        var destinationLane = TypeSystem.GetMsilVectorElementType(toType.ElementType);
        var destinationBits = LaneTypes.GetLaneBits(destinationLane);

        if (opcode is LLVMOpcode.LLVMSIToFP or LLVMOpcode.LLVMUIToFP && LaneTypes.GetLaneBits(sourceLane) == 64 && destinationLane == typeof(float))
        {
            ILGenerator.Emit(OpCodes.Ldc_I4, laneCount);
            ILGenerator.Emit(OpCodes.Call, typeof(VectorUtility)
                .GetStaticMethodStrict(opcode == LLVMOpcode.LLVMSIToFP ? nameof(VectorUtility.ConvertInt64ToSingle) : nameof(VectorUtility.ConvertUInt64ToSingle))
                .MakeGenericMethod(TypeSystem.GetMsilType(fromType), TypeSystem.GetMsilType(toType)));
            return;
        }

        // The lanes are signed (or floating point) in our representation; zero extension and unsigned
        // conversions work on the unsigned type of the same width.
        var current = opcode is LLVMOpcode.LLVMZExt or LLVMOpcode.LLVMUIToFP ? LaneTypes.GetUnsignedLaneType(sourceLane) : sourceLane;
        EmitLaneReinterpretation(laneCount, sourceLane, current);

        if (LaneTypes.IsFloatingPointLane(sourceLane) == LaneTypes.IsFloatingPointLane(destinationLane))
        {
            current = EmitLaneWidthSteps(laneCount, current, destinationBits);
        }
        else
        {
            var convertBits = Math.Max(LaneTypes.GetLaneBits(current), destinationBits);
            current = EmitLaneWidthSteps(laneCount, current, convertBits);

            var converted = LaneTypes.IsFloatingPointLane(destinationLane)
                ? (convertBits == 32 ? typeof(float) : typeof(double))
                : LaneTypes.GetIntegerLaneType(convertBits, signed: opcode != LLVMOpcode.LLVMFPToUI);
            EmitLaneKindConversion(laneCount, current, converted);

            current = EmitLaneWidthSteps(laneCount, converted, destinationBits);
        }

        EmitLaneReinterpretation(laneCount, current, destinationLane);
    }

    /// <summary>Widens or narrows the lanes of the vector on the stack until they are <paramref name="targetBits"/> wide.</summary>
    private Type EmitLaneWidthSteps(int laneCount, Type laneType, int targetBits)
    {
        while (LaneTypes.GetLaneBits(laneType) != targetBits)
        {
            var narrow = targetBits < LaneTypes.GetLaneBits(laneType);
            if (!CanEmitLaneWidthStep(laneCount, laneType, narrow))
            {
                throw new NotImplementedException($"Vector conversion of {laneCount} lanes of {laneType.Name} to {targetBits} bits.");
            }

            EmitLaneWidthStep(laneCount, laneType, narrow);
            laneType = narrow ? LaneTypes.GetNarrowerLaneType(laneType)! : LaneTypes.GetWiderLaneType(laneType)!;
        }

        return laneType;
    }

    private bool CanEmitLaneWidthStep(int laneCount, Type laneType, bool narrow)
    {
        var bits = LaneTypes.GetLaneBits(laneType);
        if (narrow)
        {
            // The BCL narrows integers and double. A Vector1024 is narrowed through its two halves.
            return LaneTypes.GetNarrowerLaneType(laneType) != null && GetContainerBits(laneCount, bits) <= 1024;
        }

        // A Vector1024 is made of two Vector512, so widening stops at 1024 bits.
        return LaneTypes.GetWiderLaneType(laneType) != null && GetContainerBits(laneCount, bits * 2) <= 1024;
    }

    private void EmitLaneWidthStep(int laneCount, Type laneType, bool narrow)
    {
        if (narrow)
        {
            EmitLaneNarrowStep(laneCount, laneType);
        }
        else
        {
            EmitLaneWidenStep(laneCount, laneType);
        }
    }

    /// <summary>
    /// <c>Vector256.Create(Vector128.WidenLower(v), Vector128.WidenUpper(v))</c>: the lane count stays and the lane
    /// width doubles. When the lanes that count are in the lower half of a <c>Vector64</c> (<c>&lt;2 x i8&gt;</c>, …),
    /// the result fits in one too, and <c>Vector64.WidenLower(v)</c> is enough.
    /// </summary>
    private void EmitLaneWidenStep(int laneCount, Type laneType)
    {
        var bits = LaneTypes.GetLaneBits(laneType);
        var container = GetContainerBits(laneCount, bits);
        var widerContainer = GetContainerBits(laneCount, bits * 2);
        var vectorClass = GetVectorClass(container);
        var vector = GetVectorOf(container, laneType);

        if (widerContainer == container)
        {
            ILGenerator.Emit(OpCodes.Call, vectorClass.GetMethodStrict(nameof(Vector128.WidenLower), [vector]));
            return;
        }

        if (widerContainer != container * 2)
        {
            throw new NotImplementedException($"Widening {laneCount} lanes of {laneType.Name}.");
        }

        var local = ILGenerator.DeclareLocal(vector);
        ILGenerator.Emit(OpCodes.Stloc, local);
        ILGenerator.Emit(OpCodes.Ldloc, local);
        ILGenerator.Emit(OpCodes.Call, vectorClass.GetMethodStrict(nameof(Vector128.WidenLower), [vector]));
        ILGenerator.Emit(OpCodes.Ldloc, local);
        ILGenerator.Emit(OpCodes.Call, vectorClass.GetMethodStrict(nameof(Vector128.WidenUpper), [vector]));

        var widened = GetVectorOf(container, LaneTypes.GetWiderLaneType(laneType)!);
        var wideLane = LaneTypes.GetWiderLaneType(laneType)!;
        ILGenerator.Emit(OpCodes.Call, widerContainer == 1024
            ? typeof(Vector1024).GetStaticMethodStrict(nameof(Vector1024.Create)).MakeGenericMethod(wideLane)
            : GetVectorClass(widerContainer).GetMethodStrict(nameof(Vector128.Create), [widened, widened]));
    }

    /// <summary>
    /// <c>Vector128.Narrow(v.GetLower(), v.GetUpper())</c>: the lane count stays and the lane width halves. A
    /// <c>Vector64</c> can't be split, so it is narrowed with itself and the lanes that count stay in its lower half.
    /// </summary>
    private void EmitLaneNarrowStep(int laneCount, Type laneType)
    {
        var bits = LaneTypes.GetLaneBits(laneType);
        var container = GetContainerBits(laneCount, bits);
        var vector = GetVectorOf(container, laneType);

        if (container <= 64)
        {
            ILGenerator.Emit(OpCodes.Dup);
            ILGenerator.Emit(OpCodes.Call, typeof(Vector64).GetMethodStrict(nameof(Vector128.Narrow), [vector, vector]));
            return;
        }

        if (GetContainerBits(laneCount, bits / 2) != container / 2)
        {
            throw new NotImplementedException($"Narrowing {laneCount} lanes of {laneType.Name}.");
        }

        var vectorClass = GetVectorClass(container);
        var half = GetVectorOf(container / 2, laneType);
        var local = ILGenerator.DeclareLocal(vector);
        ILGenerator.Emit(OpCodes.Stloc, local);
        ILGenerator.Emit(OpCodes.Ldloc, local);
        ILGenerator.Emit(OpCodes.Call, vectorClass.GetStaticMethodStrict(nameof(Vector128.GetLower)).MakeGenericMethod(laneType));
        ILGenerator.Emit(OpCodes.Ldloc, local);
        ILGenerator.Emit(OpCodes.Call, vectorClass.GetStaticMethodStrict(nameof(Vector128.GetUpper)).MakeGenericMethod(laneType));
        ILGenerator.Emit(OpCodes.Call, GetVectorClass(container / 2).GetMethodStrict(nameof(Vector128.Narrow), [half, half]));
    }

    /// <summary>Converts the lanes between floating point and integer of one width, e.g. <c>Vector128.ConvertToInt32(Vector128&lt;float&gt;)</c>.</summary>
    private void EmitLaneKindConversion(int laneCount, Type fromLane, Type toLane)
    {
        var container = GetContainerBits(laneCount, LaneTypes.GetLaneBits(fromLane));
        var methodName = Type.GetTypeCode(toLane) switch
        {
            TypeCode.Single => nameof(Vector128.ConvertToSingle),
            TypeCode.Double => nameof(Vector128.ConvertToDouble),
            TypeCode.Int32 => nameof(Vector128.ConvertToInt32),
            TypeCode.UInt32 => nameof(Vector128.ConvertToUInt32),
            TypeCode.Int64 => nameof(Vector128.ConvertToInt64),
            TypeCode.UInt64 => nameof(Vector128.ConvertToUInt64),
            _ => throw new NotImplementedException($"Vector conversion to {toLane.Name} lanes."),
        };
        ILGenerator.Emit(OpCodes.Call, GetVectorClass(container).GetMethodStrict(methodName, [GetVectorOf(container, fromLane)]));
    }

    /// <summary>Reinterprets the lanes as another type of the same width, e.g. <c>Vector128.As&lt;int, uint&gt;(v)</c>.</summary>
    private void EmitLaneReinterpretation(int laneCount, Type fromLane, Type toLane)
    {
        if (fromLane == toLane)
        {
            return;
        }

        var container = GetContainerBits(laneCount, LaneTypes.GetLaneBits(fromLane));
        ILGenerator.Emit(OpCodes.Call, GetVectorClass(container).GetStaticMethodStrict(nameof(Vector128.As)).MakeGenericMethod(fromLane, toLane));
    }

    /// <summary>
    /// The size in bits of the .NET vector that holds <paramref name="laneCount"/> lanes: the smallest hardware
    /// vector is 64 bits, and a size that isn't a power of two (<c>&lt;3 x float&gt;</c>) is rounded up.
    /// </summary>
    private static int GetContainerBits(int laneCount, int laneBits) =>
        Math.Max(64, TypeSystem.RoundUpToTypeSize(laneCount * laneBits));

    private static Type GetVectorClass(int containerBits) => containerBits switch
    {
        64 => typeof(Vector64),
        128 => typeof(Vector128),
        256 => typeof(Vector256),
        512 => typeof(Vector512),
        1024 => typeof(Vector1024),
        _ => throw new NotImplementedException($"Vectors of {containerBits} bits."),
    };

    private static Type GetVectorOf(int containerBits, Type laneType) =>
        (containerBits switch
        {
            64 => typeof(Vector64<>),
            128 => typeof(Vector128<>),
            256 => typeof(Vector256<>),
            512 => typeof(Vector512<>),
            1024 => typeof(Vector1024<>),
            _ => throw new NotImplementedException($"Vectors of {containerBits} bits."),
        }).MakeGenericType(laneType);
}

