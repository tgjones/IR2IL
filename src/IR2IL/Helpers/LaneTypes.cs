using System;

namespace IR2IL.Helpers;

/// <summary>Helpers on the lane types (<c>sbyte</c> … <c>double</c>) of .NET vectors.</summary>
internal static class LaneTypes
{
    public static int GetLaneBits(Type laneType) => Type.GetTypeCode(laneType) switch
    {
        TypeCode.SByte or TypeCode.Byte => 8,
        TypeCode.Int16 or TypeCode.UInt16 => 16,
        TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Single => 32,
        TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Double => 64,
        _ => throw new NotImplementedException($"Vector lanes of type {laneType.Name}."),
    };

    public static bool IsFloatingPointLane(Type laneType) => laneType == typeof(float) || laneType == typeof(double);

    public static Type GetIntegerLaneType(int bits, bool signed) => (bits, signed) switch
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

    public static Type GetUnsignedLaneType(Type laneType) =>
        IsFloatingPointLane(laneType) ? laneType : GetIntegerLaneType(GetLaneBits(laneType), signed: false);

    /// <summary>The lane type of twice the width and the same kind, or null when there is none.</summary>
    public static Type? GetWiderLaneType(Type laneType) => Type.GetTypeCode(laneType) switch
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
    public static Type? GetNarrowerLaneType(Type laneType) => Type.GetTypeCode(laneType) switch
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
