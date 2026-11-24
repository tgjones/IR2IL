using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using IR2IL.Helpers;
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
    /// <c>i64</c> lanes to <c>float</c> are converted to <c>double</c> first, because the BCL has no direct conversion,
    /// which rounds twice (see the vector conversions item of docs/TODO.md).
    /// </summary>
    private void EmitVectorLaneConversion(LLVMOpcode opcode, LLVMTypeRef fromType, LLVMTypeRef toType)
    {
        var laneCount = (int)fromType.VectorSize;
        var sourceLane = TypeSystem.GetMsilVectorElementType(fromType.ElementType);
        var destinationLane = TypeSystem.GetMsilVectorElementType(toType.ElementType);
        var destinationBits = GetLaneBits(destinationLane);

        // The lanes are signed (or floating point) in our representation; zero extension and unsigned
        // conversions work on the unsigned type of the same width.
        var current = opcode is LLVMOpcode.LLVMZExt or LLVMOpcode.LLVMUIToFP ? GetUnsignedLaneType(sourceLane) : sourceLane;
        EmitLaneReinterpretation(laneCount, sourceLane, current);

        if (IsFloatingPointLane(sourceLane) == IsFloatingPointLane(destinationLane))
        {
            current = EmitLaneWidthSteps(laneCount, current, destinationBits);
        }
        else
        {
            var convertBits = Math.Max(GetLaneBits(current), destinationBits);
            current = EmitLaneWidthSteps(laneCount, current, convertBits);

            var converted = IsFloatingPointLane(destinationLane)
                ? (convertBits == 32 ? typeof(float) : typeof(double))
                : GetIntegerLaneType(convertBits, signed: opcode != LLVMOpcode.LLVMFPToUI);
            EmitLaneKindConversion(laneCount, current, converted);

            current = EmitLaneWidthSteps(laneCount, converted, destinationBits);
        }

        EmitLaneReinterpretation(laneCount, current, destinationLane);
    }

    /// <summary>Widens or narrows the lanes of the vector on the stack until they are <paramref name="targetBits"/> wide.</summary>
    private Type EmitLaneWidthSteps(int laneCount, Type laneType, int targetBits)
    {
        while (GetLaneBits(laneType) != targetBits)
        {
            var narrow = targetBits < GetLaneBits(laneType);
            if (!CanEmitLaneWidthStep(laneCount, laneType, narrow))
            {
                throw new NotImplementedException($"Vector conversion of {laneCount} lanes of {laneType.Name} to {targetBits} bits.");
            }

            EmitLaneWidthStep(laneCount, laneType, narrow);
            laneType = narrow ? GetNarrowerLaneType(laneType)! : GetWiderLaneType(laneType)!;
        }

        return laneType;
    }

    private bool CanEmitLaneWidthStep(int laneCount, Type laneType, bool narrow)
    {
        var bits = GetLaneBits(laneType);
        if (narrow)
        {
            // The BCL narrows integers and double, and the vector must not be a Vector1024, which has no Narrow.
            return GetNarrowerLaneType(laneType) != null && GetContainerBits(laneCount, bits) <= 512;
        }

        // A widened vector must still fit in 512 bits, as Vector1024 has no WidenLower/WidenUpper.
        return GetWiderLaneType(laneType) != null && GetContainerBits(laneCount, bits * 2) <= 512;
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
        var bits = GetLaneBits(laneType);
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

        var widened = GetVectorOf(container, GetWiderLaneType(laneType)!);
        ILGenerator.Emit(OpCodes.Call, GetVectorClass(widerContainer).GetMethodStrict(nameof(Vector128.Create), [widened, widened]));
    }

    /// <summary>
    /// <c>Vector128.Narrow(v.GetLower(), v.GetUpper())</c>: the lane count stays and the lane width halves. A
    /// <c>Vector64</c> can't be split, so it is narrowed with itself and the lanes that count stay in its lower half.
    /// </summary>
    private void EmitLaneNarrowStep(int laneCount, Type laneType)
    {
        var bits = GetLaneBits(laneType);
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
        var container = GetContainerBits(laneCount, GetLaneBits(fromLane));
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

        var container = GetContainerBits(laneCount, GetLaneBits(fromLane));
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
        _ => throw new NotImplementedException($"Vectors of {containerBits} bits."),
    };

    private static Type GetVectorOf(int containerBits, Type laneType) =>
        (containerBits switch
        {
            64 => typeof(Vector64<>),
            128 => typeof(Vector128<>),
            256 => typeof(Vector256<>),
            512 => typeof(Vector512<>),
            _ => throw new NotImplementedException($"Vectors of {containerBits} bits."),
        }).MakeGenericType(laneType);

    private static int GetLaneBits(Type laneType) => Type.GetTypeCode(laneType) switch
    {
        TypeCode.SByte or TypeCode.Byte => 8,
        TypeCode.Int16 or TypeCode.UInt16 => 16,
        TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Single => 32,
        TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Double => 64,
        _ => throw new NotImplementedException($"Vector lanes of type {laneType.Name}."),
    };

    private static bool IsFloatingPointLane(Type laneType) => laneType == typeof(float) || laneType == typeof(double);

    private static Type GetIntegerLaneType(int bits, bool signed) => (bits, signed) switch
    {
        (8, true) => typeof(sbyte),
        (8, false) => typeof(byte),
        (16, true) => typeof(short),
        (16, false) => typeof(ushort),
        (32, true) => typeof(int),
        (32, false) => typeof(uint),
        (64, true) => typeof(long),
        (64, false) => typeof(ulong),
        _ => throw new NotImplementedException($"Vector lanes of {bits} bits."),
    };

    private static Type GetUnsignedLaneType(Type laneType) =>
        IsFloatingPointLane(laneType) ? laneType : GetIntegerLaneType(GetLaneBits(laneType), signed: false);

    /// <summary>The lane type of twice the width and the same kind, or null when there is none.</summary>
    private static Type? GetWiderLaneType(Type laneType) => Type.GetTypeCode(laneType) switch
    {
        TypeCode.SByte => typeof(short),
        TypeCode.Byte => typeof(ushort),
        TypeCode.Int16 => typeof(int),
        TypeCode.UInt16 => typeof(uint),
        TypeCode.Int32 => typeof(long),
        TypeCode.UInt32 => typeof(ulong),
        TypeCode.Single => typeof(double),
        _ => null,
    };

    /// <summary>The lane type of half the width and the same kind, or null when there is none.</summary>
    private static Type? GetNarrowerLaneType(Type laneType) => Type.GetTypeCode(laneType) switch
    {
        TypeCode.Int16 => typeof(sbyte),
        TypeCode.UInt16 => typeof(byte),
        TypeCode.Int32 => typeof(short),
        TypeCode.UInt32 => typeof(ushort),
        TypeCode.Int64 => typeof(int),
        TypeCode.UInt64 => typeof(uint),
        TypeCode.Double => typeof(float),
        _ => null,
    };
}
