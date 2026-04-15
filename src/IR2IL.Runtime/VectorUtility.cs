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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> FPToSIV2F64ToV2I64(Vector128<double> vector)
    {
        return Vector128.ConvertToInt64(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> FPToSIV4F64ToV4I32(Vector256<double> vector)
    {
        var longs = Vector256.ConvertToInt64(vector);
        return Vector128.Narrow(longs.GetLower(), longs.GetUpper());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> FPToSIV4F32ToV4I64(Vector128<float> vector)
    {
        var (lower, upper) = Vector128.Widen(vector);
        return Vector256.Create(Vector128.ConvertToInt64(lower), Vector128.ConvertToInt64(upper));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<long> SExtV8I8ToV8I64(Vector64<sbyte> vector)
    {
        // i8 → i16 → i32 → i64
        var (i16lo, i16hi) = Vector128.Widen(vector.ToVector128());
        var (i32_0, i32_1) = Vector128.Widen(i16lo);
        var (i32_2, i32_3) = Vector128.Widen(i16hi);
        var (i64_0, i64_1) = Vector128.Widen(i32_0);
        var (i64_2, i64_3) = Vector128.Widen(i32_1);
        var (i64_4, i64_5) = Vector128.Widen(i32_2);
        var (i64_6, i64_7) = Vector128.Widen(i32_3);
        return Vector512.Create(
            Vector512.Create(Vector256.Create(i64_0, i64_1), Vector256.Create(i64_2, i64_3)).GetLower(),
            Vector512.Create(Vector256.Create(i64_4, i64_5), Vector256.Create(i64_6, i64_7)).GetLower());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<long> SExtV16I8ToV16I64(Vector128<sbyte> vector)
    {
        // i8 → i16 → i32 → i64
        var (i16lo, i16hi) = Vector128.Widen(vector);
        var (i32lo_lo, i32lo_hi) = Vector128.Widen(i16lo);
        var (i32hi_lo, i32hi_hi) = Vector128.Widen(i16hi);
        var (i64_0, i64_1) = Vector128.Widen(i32lo_lo);
        var (i64_2, i64_3) = Vector128.Widen(i32lo_hi);
        var (i64_4, i64_5) = Vector128.Widen(i32hi_lo);
        var (i64_6, i64_7) = Vector128.Widen(i32hi_hi);
        return Vector512.Create(
            Vector512.Create(Vector256.Create(i64_0, i64_1), Vector256.Create(i64_2, i64_3)).GetLower(),
            Vector512.Create(Vector256.Create(i64_4, i64_5), Vector256.Create(i64_6, i64_7)).GetLower());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> SExtV8I16ToV8I32(Vector128<short> vector)
    {
        var (lower, upper) = Vector128.Widen(vector);
        return Vector256.Create(lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<long> SExtV8I16ToV8I64(Vector128<short> vector)
    {
        // i16 → i32 → i64, lanes 0-3 from lower half, 4-7 from upper half
        var (i32lo, i32hi) = Vector128.Widen(vector);
        var (i64_0, i64_1) = Vector128.Widen(i32lo);
        var (i64_2, i64_3) = Vector128.Widen(i32hi);
        return Vector512.Create(Vector256.Create(i64_0, i64_1), Vector256.Create(i64_2, i64_3));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> SExtV4I32ToV4I64(Vector128<int> vector)
    {
        var (lower, upper) = Vector128.Widen(vector);
        return Vector256.Create(lower, upper);
    }

    // Log10

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> Log10V2F32(Vector64<float> vector)
    {
        return Vector64.Log2(vector) * (1f / MathF.Log2(10f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Log10V2F64(Vector128<double> vector)
    {
        return Vector128.Log2(vector) * (1.0 / Math.Log2(10.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Log10V4F32(Vector128<float> vector)
    {
        return Vector128.Log2(vector) * (1f / MathF.Log2(10f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Log10V4F64(Vector256<double> vector)
    {
        return Vector256.Log2(vector) * (1.0 / Math.Log2(10.0));
    }

    // Pow

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> PowV2F32(Vector64<float> left, Vector64<float> right)
    {
        return Vector64.Create(MathF.Pow(left[0], right[0]), MathF.Pow(left[1], right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> PowV2F64(Vector128<double> left, Vector128<double> right)
    {
        return Vector128.Create(Math.Pow(left[0], right[0]), Math.Pow(left[1], right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> PowV4F32(Vector128<float> left, Vector128<float> right)
    {
        return Vector128.Create(MathF.Pow(left[0], right[0]), MathF.Pow(left[1], right[1]), MathF.Pow(left[2], right[2]), MathF.Pow(left[3], right[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> PowV4F64(Vector256<double> left, Vector256<double> right)
    {
        return Vector256.Create(Math.Pow(left[0], right[0]), Math.Pow(left[1], right[1]), Math.Pow(left[2], right[2]), Math.Pow(left[3], right[3]));
    }

    // SIToFP

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SIToFPV2I32ToV2F64(Vector64<int> vector)
    {
        var (lower, upper) = Vector64.Widen(vector);
        return Vector128.ConvertToDouble(Vector128.Create(lower, upper));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> SIToFPV2I32ToV2F32(Vector64<int> vector) => Vector64.ConvertToSingle(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> SIToFPV4I32ToV4F32(Vector128<int> vector) => Vector128.ConvertToSingle(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> SIToFPV4I32ToV4F64(Vector128<int> vector)
    {
        var (lower, upper) = Vector128.Widen(vector);
        return Vector256.ConvertToDouble(Vector256.Create(lower, upper));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SIToFPV2I64ToV2F64(Vector128<long> vector)
    {
        return Vector128.ConvertToDouble(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> SIToFPV4I64ToV4F32(Vector256<long> vector)
    {
        var doubles = Vector256.ConvertToDouble(vector);
        return Vector128.Narrow(doubles.GetLower(), doubles.GetUpper());
    }

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
    public static Vector64<int> SignedRemainderV2I32(Vector64<int> left, Vector64<int> right)
    {
        return Vector64.Create(left[0] % right[0], left[1] % right[1]);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> SignedRemainderV2I64(Vector128<long> left, Vector128<long> right)
    {
        return Vector128.Create(left[0] % right[0], left[1] % right[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> SignedRemainderV2F32(Vector64<float> left, Vector64<float> right)
    {
        return Vector64.Create(left[0] % right[0], left[1] % right[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SignedRemainderV2F64(Vector128<double> left, Vector128<double> right)
    {
        return Vector128.Create(left[0] % right[0], left[1] % right[1]);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> TruncV8I64ToV8I32(Vector512<long> vector)
    {
        return Vector256.Narrow(vector.GetLower(), vector.GetUpper());
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
    public static Vector256<double> UIToFPV4I32ToV4F64(Vector128<int> vector)
    {
        var (lower, upper) = Vector128.Widen(vector.AsUInt32());
        return Vector256.ConvertToDouble(Vector256.Create(lower, upper));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> UIToFPV4I64ToV4F32(Vector256<long> vector)
    {
        var doubles = Vector256.ConvertToDouble(vector.AsUInt64());
        return Vector128.Narrow(doubles.GetLower(), doubles.GetUpper());
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
    public static Vector64<int> UnsignedRemainderV2I32(Vector64<int> left, Vector64<int> right)
    {
        return Vector64.Create(
            (int)(left.AsUInt32()[0] % right.AsUInt32()[0]),
            (int)(left.AsUInt32()[1] % right.AsUInt32()[1]));
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
    public static Vector128<long> UnsignedRemainderV2I64(Vector128<long> left, Vector128<long> right)
    {
        return Vector128.Create(
            (long)(left.AsUInt64()[0] % right.AsUInt64()[0]),
            (long)(left.AsUInt64()[1] % right.AsUInt64()[1]));
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
    public static Vector64<short> ZExtV4I8ToV4I16(Vector32<sbyte> vector)
    {
        return Vector64.Create((ushort)vector[0], (ushort)vector[1], (ushort)vector[2], (ushort)vector[3]).AsInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> ZExtV8I8ToV8I32(Vector64<sbyte> vector)
    {
        // Widen bytes → ushort → uint, then reinterpret as int
        var (lo16, hi16) = Vector128.Widen(vector.AsByte().ToVector128());
        var (lo32_lo, lo32_hi) = Vector128.Widen(lo16);
        var (hi32_lo, hi32_hi) = Vector128.Widen(hi16);
        var lower = Vector256.Create(lo32_lo, lo32_hi).AsInt32();
        var upper = Vector256.Create(hi32_lo, hi32_hi).AsInt32();
        return Vector512.Create(lower, upper).GetLower();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<int> ZExtV16I16ToV16I32(Vector256<short> vector)
    {
        var (lower, upper) = Vector256.Widen(vector.AsUInt16());
        return Vector512.Create(lower.AsInt32(), upper.AsInt32());
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ZExtV2I8ToV2I64(Vector16<sbyte> vector)
    {
        return Vector128.Create((ulong)vector[0], (ulong)vector[1]).AsInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<int> ZExtV16I8ToV16I32(Vector128<sbyte> vector)
    {
        var (lower64, upper64) = Vector128.Widen(vector.AsByte());
        var (lower32a, upper32a) = Vector128.Widen(lower64);
        var (lower32b, upper32b) = Vector128.Widen(upper64);
        return Vector512.Create(
            Vector256.Create(lower32a.AsInt32(), upper32a.AsInt32()),
            Vector256.Create(lower32b.AsInt32(), upper32b.AsInt32()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> ZExtV4I32ToV4I64(Vector128<int> vector)
    {
        var (lower, upper) = Vector128.Widen(vector.AsUInt32());
        return Vector256.Create(lower.AsInt64(), upper.AsInt64());
    }
}