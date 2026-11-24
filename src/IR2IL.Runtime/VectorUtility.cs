using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

public static class VectorUtility
{
    // CompareNotEquals
    // There is no .NET cross-platform API for e.g. Vector128.NotEquals, so we simulate it.

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> CompareVector256NotEquals<T>(Vector256<T> left, Vector256<T> right)
    {
        return Vector256.OnesComplement(Vector256.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> CompareVector512NotEquals<T>(Vector512<T> left, Vector512<T> right)
    {
        return Vector512.OnesComplement(Vector512.Equals(left, right));
    }

    // IntToPtr

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<nint> IntToPtrV2I64ToV2Ptr(Vector128<long> vector)
    {
        return Vector128.Create(
            Vector64.Create((nint)vector[0]),
            Vector64.Create((nint)vector[1]));
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

    // Acos

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AcosV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Acos(vector[0]), MathF.Acos(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AcosV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Acos(vector[0]), Math.Acos(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AcosV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Acos(vector[0]), MathF.Acos(vector[1]), MathF.Acos(vector[2]), MathF.Acos(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AcosV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Acos(vector[0]), Math.Acos(vector[1]), Math.Acos(vector[2]), Math.Acos(vector[3]));
    }

    // Asin

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AsinV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Asin(vector[0]), MathF.Asin(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AsinV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Asin(vector[0]), Math.Asin(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AsinV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Asin(vector[0]), MathF.Asin(vector[1]), MathF.Asin(vector[2]), MathF.Asin(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AsinV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Asin(vector[0]), Math.Asin(vector[1]), Math.Asin(vector[2]), Math.Asin(vector[3]));
    }

    // Atan

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AtanV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Atan(vector[0]), MathF.Atan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AtanV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Atan(vector[0]), Math.Atan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AtanV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Atan(vector[0]), MathF.Atan(vector[1]), MathF.Atan(vector[2]), MathF.Atan(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AtanV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Atan(vector[0]), Math.Atan(vector[1]), Math.Atan(vector[2]), Math.Atan(vector[3]));
    }

    // Tan

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> TanV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Tan(vector[0]), MathF.Tan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> TanV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Tan(vector[0]), Math.Tan(vector[1]));
    }

    // Sinh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> SinhV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Sinh(vector[0]), MathF.Sinh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SinhV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Sinh(vector[0]), Math.Sinh(vector[1]));
    }

    // Cosh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> CoshV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Cosh(vector[0]), MathF.Cosh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> CoshV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Cosh(vector[0]), Math.Cosh(vector[1]));
    }

    // Tanh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> TanhV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Tanh(vector[0]), MathF.Tanh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> TanhV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Tanh(vector[0]), Math.Tanh(vector[1]));
    }

    // Atan2

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> Atan2V2F32(Vector64<float> y, Vector64<float> x)
    {
        return Vector64.Create(MathF.Atan2(y[0], x[0]), MathF.Atan2(y[1], x[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Atan2V4F32(Vector128<float> y, Vector128<float> x)
    {
        return Vector128.Create(MathF.Atan2(y[0], x[0]), MathF.Atan2(y[1], x[1]), MathF.Atan2(y[2], x[2]), MathF.Atan2(y[3], x[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Atan2V2F64(Vector128<double> y, Vector128<double> x)
    {
        return Vector128.Create(Math.Atan2(y[0], x[0]), Math.Atan2(y[1], x[1]));
    }

    // Remainder
    // `srem`, `urem` and `frem` of vectors. There is no SIMD division or remainder, so each lane is computed on its
    // own; one generic method serves every vector type and lane type. Only the real lanes are computed: the
    // padding lanes of a vector may hold anything, such as a zero divisor.

    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector Remainder<TVector, TLane>(TVector left, TVector right, int laneCount)
        where TVector : struct
        where TLane : struct, IModulusOperators<TLane, TLane, TLane>
    {
        var result = default(TVector);
        ref var l = ref Unsafe.As<TVector, TLane>(ref left);
        ref var r = ref Unsafe.As<TVector, TLane>(ref right);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref l, i) % Unsafe.Add(ref r, i);
        }

        return result;
    }

    // MaskFromBits: bitcast iN to <N x i1>. Lane i is all ones (-1) when bit i is set, 0 otherwise,
    // the same representation as the result of a vector comparison.

    // The masks of 2 and 4 lanes are held in a Vector64<sbyte> with 8 lanes, like the one of 8 lanes.
    // The lanes after the real ones are undefined, so they can come from the bits above them.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits2(ulong bits) => MaskFromBits8(bits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits4(ulong bits) => MaskFromBits8(bits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits8(ulong bits) =>
        Vector64.Create(
            MaskLane(bits, 0), MaskLane(bits, 1), MaskLane(bits, 2), MaskLane(bits, 3),
            MaskLane(bits, 4), MaskLane(bits, 5), MaskLane(bits, 6), MaskLane(bits, 7));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> MaskFromBits16(ulong bits) =>
        Vector128.Create(MaskFromBits8(bits), MaskFromBits8(bits >> 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> MaskFromBits32(ulong bits) =>
        Vector256.Create(MaskFromBits16(bits), MaskFromBits16(bits >> 16));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<sbyte> MaskFromBits64(ulong bits) =>
        Vector512.Create(MaskFromBits32(bits), MaskFromBits32(bits >> 32));

    private static sbyte MaskLane(ulong bits, int index) => (sbyte)-(int)((bits >> index) & 1);

    // NormalizeI1Lanes

    // An <N x i1> mask is stored as sbyte lanes, and a true lane is all ones (-1). zext reads only the low bit,
    // so before it is extended (see FunctionILEmitter.EmitVectorLaneConversion) it clears the rest.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> NormalizeI1Lanes(Vector64<sbyte> vector) => vector & Vector64<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> NormalizeI1Lanes(Vector128<sbyte> vector) => vector & Vector128<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> NormalizeI1Lanes(Vector256<sbyte> vector) => vector & Vector256<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<sbyte> NormalizeI1Lanes(Vector512<sbyte> vector) => vector & Vector512<sbyte>.One;

    // Scmp / Ucmp

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Scmp(int a, int b) => a < b ? -1 : a > b ? 1 : 0;
}