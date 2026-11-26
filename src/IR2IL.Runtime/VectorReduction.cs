using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace IR2IL.Runtime;

/// <summary>
/// Reductions of the lanes of a vector to one value (<c>llvm.vector.reduce.mul/and/or/xor/smax/smin/umax/umin</c>) that
/// have no cross-platform BCL method (<c>Sum</c> does). Signedness comes from the lane type. Every method takes a concrete
/// <c>Vector64&lt;T&gt;</c> … <c>Vector1024&lt;T&gt;</c>, never reinterprets one through a reference, and is
/// <c>AggressiveInlining</c>, so the JIT keeps the vector in registers. Wider vectors are halved with the element-wise
/// operation first; <c>Vector128</c> and <c>Vector64</c> use the across-lanes instructions of ARM64 (and
/// <c>PHMINPOSUW</c> on x86 for 16-bit lanes) for <c>Max</c> and <c>Min</c>, and a rotate-and-combine sequence otherwise.
/// </summary>
public static class VectorReduction
{
    private interface IOperation<T> where T : unmanaged
    {
        static abstract Vector64<T> Apply(Vector64<T> a, Vector64<T> b);
        static abstract Vector128<T> Apply(Vector128<T> a, Vector128<T> b);
    }

    private readonly struct MaxOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => Vector64.Max(a, b);
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => Vector128.Max(a, b);
    }

    private readonly struct MinOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => Vector64.Min(a, b);
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => Vector128.Min(a, b);
    }

    private readonly struct MultiplyOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => a * b;
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => a * b;
    }

    private readonly struct AndOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => a & b;
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => a & b;
    }

    private readonly struct OrOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => a | b;
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => a | b;
    }

    private readonly struct XorOperation<T> : IOperation<T> where T : unmanaged
    {
        public static Vector64<T> Apply(Vector64<T> a, Vector64<T> b) => a ^ b;
        public static Vector128<T> Apply(Vector128<T> a, Vector128<T> b) => a ^ b;
    }

    // Each step combines the vector with itself rotated by half of what is left (8, 4, 2, 1 bytes); once the
    // rotation is smaller than a lane, lane 0 holds the result.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Rotate<T, TOperation>(Vector128<T> v)
        where T : unmanaged
        where TOperation : IOperation<T>
    {
        v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7)).As<byte, T>());
        if (Unsafe.SizeOf<T>() <= 4)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3)).As<byte, T>());
        }

        if (Unsafe.SizeOf<T>() <= 2)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1)).As<byte, T>());
        }

        if (Unsafe.SizeOf<T>() == 1)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0)).As<byte, T>());
        }

        return v.ToScalar();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Rotate<T, TOperation>(Vector64<T> v)
        where T : unmanaged
        where TOperation : IOperation<T>
    {
        if (Unsafe.SizeOf<T>() <= 4)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)4, 5, 6, 7, 0, 1, 2, 3)).As<byte, T>());
        }

        if (Unsafe.SizeOf<T>() <= 2)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)2, 3, 4, 5, 6, 7, 0, 1)).As<byte, T>());
        }

        if (Unsafe.SizeOf<T>() == 1)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)1, 2, 3, 4, 5, 6, 7, 0)).As<byte, T>());
        }

        return v.ToScalar();
    }

    // Max

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(Vector64<T> vector) where T : unmanaged
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            if (typeof(T) == typeof(sbyte)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsSByte()).ToScalar();
            if (typeof(T) == typeof(byte)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsByte()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsInt16()).ToScalar();
            if (typeof(T) == typeof(ushort)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(int)) return (T)(object)AdvSimd.MaxPairwise(vector.AsInt32(), vector.AsInt32()).ToScalar();
            if (typeof(T) == typeof(uint)) return (T)(object)AdvSimd.MaxPairwise(vector.AsUInt32(), vector.AsUInt32()).ToScalar();
        }
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, MaxOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(Vector128<T> vector) where T : unmanaged
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            if (typeof(T) == typeof(sbyte)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsSByte()).ToScalar();
            if (typeof(T) == typeof(byte)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsByte()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsInt16()).ToScalar();
            if (typeof(T) == typeof(ushort)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(int)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsInt32()).ToScalar();
            if (typeof(T) == typeof(uint)) return (T)(object)AdvSimd.Arm64.MaxAcross(vector.AsUInt32()).ToScalar();
        }

        if (Sse41.IsSupported)
        {
            if (typeof(T) == typeof(ushort)) return (T)(object)(ushort)~Sse41.MinHorizontal(~vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)(short)(Sse41.MinHorizontal(vector.AsUInt16() ^ Vector128.Create((ushort)0x7FFF)).ToScalar() ^ 0x7FFF);
        }
        return Rotate<T, MaxOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(Vector256<T> vector) where T : unmanaged => Max(Vector128.Max(vector.GetLower(), vector.GetUpper()));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(Vector512<T> vector) where T : unmanaged => Max(Vector256.Max(vector.GetLower(), vector.GetUpper()));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(Vector1024<T> vector) where T : unmanaged => Max(Vector512.Max(vector.GetLower(), vector.GetUpper()));

    // Min

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(Vector64<T> vector) where T : unmanaged
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            if (typeof(T) == typeof(sbyte)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsSByte()).ToScalar();
            if (typeof(T) == typeof(byte)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsByte()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsInt16()).ToScalar();
            if (typeof(T) == typeof(ushort)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(int)) return (T)(object)AdvSimd.MinPairwise(vector.AsInt32(), vector.AsInt32()).ToScalar();
            if (typeof(T) == typeof(uint)) return (T)(object)AdvSimd.MinPairwise(vector.AsUInt32(), vector.AsUInt32()).ToScalar();
        }
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, MinOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(Vector128<T> vector) where T : unmanaged
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            if (typeof(T) == typeof(sbyte)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsSByte()).ToScalar();
            if (typeof(T) == typeof(byte)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsByte()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsInt16()).ToScalar();
            if (typeof(T) == typeof(ushort)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(int)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsInt32()).ToScalar();
            if (typeof(T) == typeof(uint)) return (T)(object)AdvSimd.Arm64.MinAcross(vector.AsUInt32()).ToScalar();
        }

        if (Sse41.IsSupported)
        {
            if (typeof(T) == typeof(ushort)) return (T)(object)Sse41.MinHorizontal(vector.AsUInt16()).ToScalar();
            if (typeof(T) == typeof(short)) return (T)(object)(short)(Sse41.MinHorizontal(vector.AsUInt16() ^ Vector128.Create((ushort)0x8000)).ToScalar() ^ 0x8000);
        }
        return Rotate<T, MinOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(Vector256<T> vector) where T : unmanaged => Min(Vector128.Min(vector.GetLower(), vector.GetUpper()));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(Vector512<T> vector) where T : unmanaged => Min(Vector256.Min(vector.GetLower(), vector.GetUpper()));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(Vector1024<T> vector) where T : unmanaged => Min(Vector512.Min(vector.GetLower(), vector.GetUpper()));

    // Multiply

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(Vector64<T> vector) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, MultiplyOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(Vector128<T> vector) where T : unmanaged
    {
        return Rotate<T, MultiplyOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(Vector256<T> vector) where T : unmanaged => Multiply(vector.GetLower() * vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(Vector512<T> vector) where T : unmanaged => Multiply(vector.GetLower() * vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(Vector1024<T> vector) where T : unmanaged => Multiply(vector.GetLower() * vector.GetUpper());

    // And

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(Vector64<T> vector) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, AndOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(Vector128<T> vector) where T : unmanaged
    {
        return Rotate<T, AndOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(Vector256<T> vector) where T : unmanaged => And(vector.GetLower() & vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(Vector512<T> vector) where T : unmanaged => And(vector.GetLower() & vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(Vector1024<T> vector) where T : unmanaged => And(vector.GetLower() & vector.GetUpper());

    // Or

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(Vector64<T> vector) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, OrOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(Vector128<T> vector) where T : unmanaged
    {
        return Rotate<T, OrOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(Vector256<T> vector) where T : unmanaged => Or(vector.GetLower() | vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(Vector512<T> vector) where T : unmanaged => Or(vector.GetLower() | vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(Vector1024<T> vector) where T : unmanaged => Or(vector.GetLower() | vector.GetUpper());

    // Xor

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(Vector64<T> vector) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 8) return vector.ToScalar();
        return Rotate<T, XorOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(Vector128<T> vector) where T : unmanaged
    {
        return Rotate<T, XorOperation<T>>(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(Vector256<T> vector) where T : unmanaged => Xor(vector.GetLower() ^ vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(Vector512<T> vector) where T : unmanaged => Xor(vector.GetLower() ^ vector.GetUpper());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(Vector1024<T> vector) where T : unmanaged => Xor(vector.GetLower() ^ vector.GetUpper());
}
