using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

public static class VectorUtility
{
    // CompareNotEquals
    // There is no .NET cross-platform API for e.g. Vector128.NotEquals, so we simulate it.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector32<T> CompareVector32NotEquals<T>(Vector32<T> left, Vector32<T> right)
        where T : unmanaged, IBitwiseOperators<T, T, T>, IEqualityOperators<T, T, bool>
    {
        return Vector32.OnesComplement(Vector32.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> CompareVector64NotEquals<T>(Vector64<T> left, Vector64<T> right)
    {
        return Vector64.OnesComplement(Vector64.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CompareVector128NotEquals<T>(Vector128<T> left, Vector128<T> right)
    {
        return Vector128.OnesComplement(Vector128.Equals(left, right));
    }

    // FPExt

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> FPExtV2F32ToV2F64(Vector64<float> vector)
    {
        var (lower, upper) = Vector64.Widen(vector);
        return Vector128.Create(lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> FPExtV4F32ToV4F64(Vector128<float> vector)
    {
        var (lower, upper) = Vector128.Widen(vector);
        return Vector256.Create(lower, upper);
    }

    // FPToSI

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> FPToSIV2F64ToV2I32(Vector128<double> vector)
    {
        return Vector64.ConvertToInt32(Vector64.Narrow(vector.GetLower(), vector.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> FPToSIV4F32ToV4I32(Vector128<float> vector)
    {
        return Vector128.ConvertToInt32(vector);
    }

    // FPToUI

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> FPToUIV2F64ToV2I32(Vector128<double> vector)
    {
        return Vector64.ConvertToUInt32(Vector64.Narrow(vector.GetLower(), vector.GetUpper())).AsInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> FPToUIV4F32ToV4I32(Vector128<float> vector)
    {
        return Vector128.ConvertToUInt32(vector).AsInt32();
    }

    // FPTrunc

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> FPTruncV2F64ToV2F32(Vector128<double> vector)
    {
        return Vector64.Narrow(vector.GetLower(), vector.GetUpper());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> FPTruncV4F64ToV4F32(Vector256<double> vector)
    {
        return Vector128.Narrow(vector.GetLower(), vector.GetUpper());
    }

    // IntToPtr

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<nint> IntToPtrV2I64ToV2Ptr(Vector128<long> vector)
    {
        return Vector128.Create(
            Vector64.Create((nint)vector[0]),
            Vector64.Create((nint)vector[1]));
    }

    // SExt

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> SExtV2I16ToV2I32(Vector32<short> vector)
    {
        return Vector64.Create(vector[0], vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> SExtV2I16ToV2I64(Vector32<short> vector)
    {
        return Vector128.Create(vector[0], vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> SExtV2I32ToV2I64(Vector64<int> vector)
    {
        return Vector128.Create(vector[0], vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> SExtV4I16ToV4I32(Vector64<short> vector)
    {
        var (lower, upper) = Vector64.Widen(vector);
        return Vector128.Create(lower, upper);
    }

    // SIToFP

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SIToFPV2I32ToV2F64(Vector64<int> vector)
    {
        var (lower, upper) = Vector64.Widen(vector);
        return Vector128.ConvertToDouble(Vector128.Create(lower, upper));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> SIToFPV4I32ToV4F32(Vector128<int> vector) => Vector128.ConvertToSingle(vector);

    // SRem

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector16<sbyte> SignedRemainderV2I8(Vector16<sbyte> left, Vector16<sbyte> right)
    {
        // TODO: Optimize this.
        return Vector16.Create(
            (sbyte)(left[0] % right[0]),
            (sbyte)(left[1] % right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector32<short> SignedRemainderV2I16(Vector32<short> left, Vector32<short> right)
    {
        // TODO: Optimize this.
        return Vector32.Create(
            (short)(left[0] % right[0]),
            (short)(left[1] % right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<short> SignedRemainderV4I16(Vector64<short> left, Vector64<short> right)
    {
        // TODO: Optimize this.
        return Vector64.Create(
            (short)(left[0] % right[0]),
            (short)(left[1] % right[1]),
            (short)(left[2] % right[2]),
            (short)(left[3] % right[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> SignedRemainderV4I32(Vector128<int> left, Vector128<int> right)
    {
        // TODO: Optimize this.
        return Vector128.Create(
            left[0] % right[0],
            left[1] % right[1],
            left[2] % right[2],
            left[3] % right[3]);
    }

    // Trunc

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector32<short> TruncV2I64ToV2I16(Vector128<long> vector)
    {
        return Vector32.Create((short)vector[0], (short)vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> TruncV2I64ToV2I32(Vector128<long> vector)
    {
        return Vector64.Narrow(vector.GetLower(), vector.GetUpper());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> TruncV4I64ToV4I32(Vector256<long> vector)
    {
        return Vector128.Narrow(vector.GetLower(), vector.GetUpper());
    }

    // UIToFP

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> UIToFPV2I8ToV2F32(Vector16<sbyte> vector)
    {
        return Vector16.ConvertToSingle(vector.AsByte());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> UIToFPV2I32ToV2F32(Vector64<int> vector)
    {
        return Vector64.ConvertToSingle(vector.AsUInt32());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> UIToFPV2I32ToV2F64(Vector64<int> vector)
    {
        var (lower, upper) = Vector64.Widen(vector.AsUInt32());
        return Vector128.ConvertToDouble(Vector128.Create(lower, upper));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> UIToFPV2I64ToV2F32(Vector128<long> vector)
    {
        var temp = Vector128.ConvertToDouble(vector.AsUInt64());
        return Vector64.Narrow(temp.GetLower(), temp.GetUpper());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> UIToFPV2I64ToV2F64(Vector128<long> vector)
    {
        return Vector128.ConvertToDouble(vector.AsUInt64());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> UIToFPV4I32ToV4F32(Vector128<int> vector)
    {
        return Vector128.ConvertToSingle(vector.AsUInt32());
    }

    // URem

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector16<sbyte> UnsignedRemainderV2I8(Vector16<sbyte> left, Vector16<sbyte> right)
    {
        // TODO: Optimize this.

        var leftUnsigned = left.AsByte();
        var rightUnsigned = right.AsByte();

        var signedResult = Vector16.Create(
            (byte)(leftUnsigned[0] % rightUnsigned[0]),
            (byte)(leftUnsigned[1] % rightUnsigned[1]));

        return signedResult.AsSByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector32<short> UnsignedRemainderV2I16(Vector32<short> left, Vector32<int> right)
    {
        // TODO: Optimize this.

        var leftUnsigned = left.AsUInt16();
        var rightUnsigned = right.AsUInt16();

        var signedResult = Vector32.Create(
            (ushort)(leftUnsigned[0] % rightUnsigned[0]),
            (ushort)(leftUnsigned[1] % rightUnsigned[1]));

        return signedResult.AsInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<short> UnsignedRemainderV4I16(Vector64<short> left, Vector64<int> right)
    {
        // TODO: Optimize this.

        var leftUnsigned = left.AsUInt16();
        var rightUnsigned = right.AsUInt16();

        var signedResult = Vector64.Create(
            (ushort)(leftUnsigned[0] % rightUnsigned[0]),
            (ushort)(leftUnsigned[1] % rightUnsigned[1]),
            (ushort)(leftUnsigned[2] % rightUnsigned[2]),
            (ushort)(leftUnsigned[3] % rightUnsigned[3]));

        return signedResult.AsInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> UnsignedRemainderV4I32(Vector128<int> left, Vector128<int> right)
    {
        // TODO: Optimize this.

        var leftUnsigned = left.AsUInt32();
        var rightUnsigned = right.AsUInt32();

        var signedResult = Vector128.Create(
            leftUnsigned[0] % rightUnsigned[0],
            leftUnsigned[1] % rightUnsigned[1],
            leftUnsigned[2] % rightUnsigned[2],
            leftUnsigned[3] % rightUnsigned[3]);

        return signedResult.AsInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> UnsignedRemainderV8I16(Vector128<short> left, Vector128<short> right)
    {
        // TODO: Optimize this.

        var leftUnsigned = left.AsUInt16();
        var rightUnsigned = right.AsUInt16();

        var signedResult = Vector128.Create(
            (ushort)(leftUnsigned[0] % rightUnsigned[0]),
            (ushort)(leftUnsigned[1] % rightUnsigned[1]),
            (ushort)(leftUnsigned[2] % rightUnsigned[2]),
            (ushort)(leftUnsigned[3] % rightUnsigned[3]),
            (ushort)(leftUnsigned[4] % rightUnsigned[4]),
            (ushort)(leftUnsigned[5] % rightUnsigned[5]),
            (ushort)(leftUnsigned[6] % rightUnsigned[6]),
            (ushort)(leftUnsigned[7] % rightUnsigned[7]));

        return signedResult.AsInt16();
    }

    // ZExt

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> ZExtV2I16ToV2I32(Vector32<short> vector)
    {
        return Vector64.Create((ushort)vector[0], (ushort)vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ZExtV2I16ToV2I64(Vector32<short> vector)
    {
        return Vector128.Create((ushort)vector[0], (ushort)vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ZExtV2I32ToV2I64(Vector64<int> vector)
    {
        return Vector128.Create((uint)vector[0], (uint)vector[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> ZExtV4I8ToV4I32(Vector32<sbyte> vector)
    {
        return Vector128.Create((uint)vector[0], (uint)vector[1], (uint)vector[2], (uint)vector[3]).AsInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> ZExtV4I16ToV4I32(Vector64<short> vector)
    {
        var (lower, upper) = Vector64.Widen(vector.AsUInt16());
        return Vector128.Create(lower, upper).AsInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> ZExtV8I8ToV8I16(Vector64<sbyte> vector)
    {
        var (lower, upper) = Vector64.Widen(vector.AsByte());
        return Vector128.Create(lower, upper).AsInt16();
    }
}