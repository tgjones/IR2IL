using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

/// <summary>
/// The floating point math intrinsics (<c>llvm.sin.v8f32</c>, <c>llvm.pow.v4f64</c>, …) and the integer ones
/// (<c>llvm.smax.v4i32</c>, <c>llvm.ctpop.v2i64</c>, …) on a vector of any shape. The compiler picks the vector
/// type, the lane type and the operation, and one generic method per arity applies the operation to each real lane.
/// A floating point lane goes through the scalar implementation (the C library, as for the scalar intrinsics),
/// which is also what LLVM does for a vector it can't vectorise. The padding lanes of a vector are not touched.
/// The scalar integer intrinsics call the same operations, on one value.
/// </summary>
public static class VectorMath
{
    public interface IUnaryOperation<T>
    {
        static abstract T Apply(T x);
    }

    public interface IBinaryOperation<T>
    {
        static abstract T Apply(T x, T y);
    }

    /// <summary>A binary operation that also has a SIMD form at every vector width, for the reductions.</summary>
    public interface IVectorOperation<T> : IBinaryOperation<T> where T : struct
    {
        static abstract Vector64<T> Apply(Vector64<T> x, Vector64<T> y);
        static abstract Vector128<T> Apply(Vector128<T> x, Vector128<T> y);
        static abstract Vector256<T> Apply(Vector256<T> x, Vector256<T> y);
        static abstract Vector512<T> Apply(Vector512<T> x, Vector512<T> y);
    }

    public interface ITernaryOperation<T>
    {
        static abstract T Apply(T x, T y, T z);
    }

    public readonly struct Sin<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Sin(x); }
    public readonly struct Cos<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Cos(x); }
    public readonly struct Tan<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Tan(x); }
    public readonly struct Asin<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Asin(x); }
    public readonly struct Acos<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Acos(x); }
    public readonly struct Atan<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Atan(x); }
    public readonly struct Sinh<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Sinh(x); }
    public readonly struct Cosh<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Cosh(x); }
    public readonly struct Tanh<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Tanh(x); }
    public readonly struct Exp<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Exp(x); }
    public readonly struct Log<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Log(x); }
    public readonly struct Log10<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Log10(x); }
    public readonly struct Sqrt<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Sqrt(x); }
    public readonly struct Fabs<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Abs(x); }
    public readonly struct Ceil<T> : IUnaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x) => T.Ceiling(x); }
    public readonly struct Atan2<T> : IBinaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x, T y) => T.Atan2(x, y); }
    public readonly struct Pow<T> : IBinaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x, T y) => T.Pow(x, y); }
    public readonly struct CopySign<T> : IBinaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x, T y) => T.CopySign(x, y); }
    public readonly struct FusedMultiplyAdd<T> : ITernaryOperation<T> where T : IFloatingPointIeee754<T> { public static T Apply(T x, T y, T z) => T.FusedMultiplyAdd(x, y, z); }


    // Integer operations. The compiler reads the lanes as signed or unsigned by the intrinsic (`smin` or `umin`)
    // by choosing the lane type, so one generic operation serves both.

    public readonly struct IntegerAbs<T> : IUnaryOperation<T> where T : IBinaryInteger<T>, ISignedNumber<T>
    {
        // The absolute value of the minimum value is itself (or poison, when the flag says so).
        public static T Apply(T x) => T.IsNegative(x) ? unchecked(T.Zero - x) : x;
    }

    public readonly struct IntegerMin<T> : IVectorOperation<T> where T : struct, IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => T.Min(x, y);
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.Min(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.Min(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.Min(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.Min(x, y);
    }

    public readonly struct IntegerMax<T> : IVectorOperation<T> where T : struct, IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => T.Max(x, y);
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.Max(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.Max(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.Max(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.Max(x, y);
    }

    public readonly struct PopCount<T> : IUnaryOperation<T> where T : IBinaryInteger<T> { public static T Apply(T x) => T.PopCount(x); }

    /// <summary><c>usub.sat</c>: <c>x - y</c>, or zero when <c>y</c> is larger. Read as unsigned.</summary>
    public readonly struct UnsignedSubtractSaturate<T> : IBinaryOperation<T> where T : IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => x > y ? unchecked(x - y) : T.Zero;
    }

    // Operations of the reductions (`llvm.vector.reduce.*`). Integer arithmetic wraps; for a floating point lane the
    // same operators are the IEEE ones.
    public readonly struct Add<T> : IVectorOperation<T> where T : struct, INumberBase<T>
    {
        public static T Apply(T x, T y) => unchecked(x + y);
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.Add(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.Add(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.Add(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.Add(x, y);
    }

    public readonly struct Multiply<T> : IVectorOperation<T> where T : struct, INumberBase<T>
    {
        public static T Apply(T x, T y) => unchecked(x * y);
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.Multiply(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.Multiply(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.Multiply(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.Multiply(x, y);
    }

    public readonly struct BitAnd<T> : IVectorOperation<T> where T : struct, IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => x & y;
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.BitwiseAnd(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.BitwiseAnd(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.BitwiseAnd(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.BitwiseAnd(x, y);
    }

    public readonly struct BitOr<T> : IVectorOperation<T> where T : struct, IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => x | y;
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.BitwiseOr(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.BitwiseOr(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.BitwiseOr(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.BitwiseOr(x, y);
    }

    public readonly struct BitXor<T> : IVectorOperation<T> where T : struct, IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => x ^ y;
        public static Vector64<T> Apply(Vector64<T> x, Vector64<T> y) => Vector64.Xor(x, y);
        public static Vector128<T> Apply(Vector128<T> x, Vector128<T> y) => Vector128.Xor(x, y);
        public static Vector256<T> Apply(Vector256<T> x, Vector256<T> y) => Vector256.Xor(x, y);
        public static Vector512<T> Apply(Vector512<T> x, Vector512<T> y) => Vector512.Xor(x, y);
    }

    /// <summary><c>scmp</c> and <c>ucmp</c> of scalars: -1, 0 or 1, as an <see cref="int"/> for the caller to widen.</summary>
    public static int Compare<T>(T x, T y) where T : IBinaryInteger<T> => x > y ? 1 : x < y ? -1 : 0;

    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector Unary<TVector, TLane, TOperation>(TVector x, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : IUnaryOperation<TLane>
    {
        var result = default(TVector);
        ref var a = ref Unsafe.As<TVector, TLane>(ref x);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = TOperation.Apply(Unsafe.Add(ref a, i));
        }

        return result;
    }

    /// <inheritdoc cref="Unary{TVector, TLane, TOperation}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector Binary<TVector, TLane, TOperation>(TVector x, TVector y, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : IBinaryOperation<TLane>
    {
        var result = default(TVector);
        ref var a = ref Unsafe.As<TVector, TLane>(ref x);
        ref var b = ref Unsafe.As<TVector, TLane>(ref y);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = TOperation.Apply(Unsafe.Add(ref a, i), Unsafe.Add(ref b, i));
        }

        return result;
    }

    /// <inheritdoc cref="Unary{TVector, TLane, TOperation}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector Ternary<TVector, TLane, TOperation>(TVector x, TVector y, TVector z, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : ITernaryOperation<TLane>
    {
        var result = default(TVector);
        ref var a = ref Unsafe.As<TVector, TLane>(ref x);
        ref var b = ref Unsafe.As<TVector, TLane>(ref y);
        ref var c = ref Unsafe.As<TVector, TLane>(ref z);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = TOperation.Apply(Unsafe.Add(ref a, i), Unsafe.Add(ref b, i), Unsafe.Add(ref c, i));
        }

        return result;
    }

    /// <summary>
    /// A reduction (<c>llvm.vector.reduce.add</c>, …) that may combine the lanes in any order: the operation applied
    /// to the real lanes of <paramref name="x"/>. A vector whose lanes fill its container is halved with SIMD
    /// operations (<c>Vector512</c> to <c>Vector256</c> to <c>Vector128</c> to <c>Vector64</c>) and then rotated
    /// within the register; one with padding lanes (<c>&lt;3 x i32&gt;</c>) or wider than 512 bits is a scalar loop.
    /// </summary>
    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    public static TLane Reduce<TVector, TLane, TOperation>(TVector x, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : IVectorOperation<TLane>
    {
        if (laneCount * Unsafe.SizeOf<TLane>() == Unsafe.SizeOf<TVector>())
        {
            switch (Unsafe.SizeOf<TVector>())
            {
                case 8:
                    return ReduceVector64<TLane, TOperation>(Unsafe.As<TVector, Vector64<TLane>>(ref x));
                case 16:
                    return ReduceVector128<TLane, TOperation>(Unsafe.As<TVector, Vector128<TLane>>(ref x));
                case 32:
                    var v256 = Unsafe.As<TVector, Vector256<TLane>>(ref x);
                    return ReduceVector128<TLane, TOperation>(TOperation.Apply(v256.GetLower(), v256.GetUpper()));
                case 64:
                    var v512 = Unsafe.As<TVector, Vector512<TLane>>(ref x);
                    var half = TOperation.Apply(v512.GetLower(), v512.GetUpper());
                    return ReduceVector128<TLane, TOperation>(TOperation.Apply(half.GetLower(), half.GetUpper()));
            }
        }

        ref var a = ref Unsafe.As<TVector, TLane>(ref x);
        var result = a;
        for (var i = 1; i < laneCount; i++)
        {
            result = TOperation.Apply(result, Unsafe.Add(ref a, i));
        }

        return result;
    }

    // Each step combines the vector with itself rotated by half of what is left (8, 4, 2, 1 bytes); once the
    // rotation is smaller than a lane, lane 0 holds the result.
    private static TLane ReduceVector128<TLane, TOperation>(Vector128<TLane> v)
        where TLane : struct
        where TOperation : IVectorOperation<TLane>
    {
        v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7)).As<byte, TLane>());
        if (Unsafe.SizeOf<TLane>() <= 4)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3)).As<byte, TLane>());
        }

        if (Unsafe.SizeOf<TLane>() <= 2)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1)).As<byte, TLane>());
        }

        if (Unsafe.SizeOf<TLane>() == 1)
        {
            v = TOperation.Apply(v, Vector128.Shuffle(v.AsByte(), Vector128.Create((byte)1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0)).As<byte, TLane>());
        }

        return v.ToScalar();
    }

    private static TLane ReduceVector64<TLane, TOperation>(Vector64<TLane> v)
        where TLane : struct
        where TOperation : IVectorOperation<TLane>
    {
        if (Unsafe.SizeOf<TLane>() <= 4)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)4, 5, 6, 7, 0, 1, 2, 3)).As<byte, TLane>());
        }

        if (Unsafe.SizeOf<TLane>() <= 2)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)2, 3, 4, 5, 6, 7, 0, 1)).As<byte, TLane>());
        }

        if (Unsafe.SizeOf<TLane>() == 1)
        {
            v = TOperation.Apply(v, Vector64.Shuffle(v.AsByte(), Vector64.Create((byte)1, 2, 3, 4, 5, 6, 7, 0)).As<byte, TLane>());
        }

        return v.ToScalar();
    }

    /// <summary>
    /// A floating point reduction (<c>llvm.vector.reduce.fadd</c>, <c>fmul</c>) with the <c>reassoc</c> flag: the
    /// lanes may be combined in any order, so they go through <see cref="Reduce{TVector, TLane, TOperation}"/>.
    /// </summary>
    public static TLane ReduceReassociated<TVector, TLane, TOperation>(TLane start, TVector x, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : IVectorOperation<TLane>
        => TOperation.Apply(start, Reduce<TVector, TLane, TOperation>(x, laneCount));

    /// <summary>
    /// An ordered floating point reduction (<c>llvm.vector.reduce.fadd</c>, <c>fmul</c> without <c>reassoc</c>): the
    /// operation applied to <paramref name="start"/> and then each real lane in turn. It can't use SIMD: the result
    /// depends on the order.
    /// </summary>
    public static TLane ReduceOrdered<TVector, TLane, TOperation>(TLane start, TVector x, int laneCount)
        where TVector : struct
        where TLane : struct
        where TOperation : IBinaryOperation<TLane>
    {
        ref var a = ref Unsafe.As<TVector, TLane>(ref x);
        var result = start;
        for (var i = 0; i < laneCount; i++)
        {
            result = TOperation.Apply(result, Unsafe.Add(ref a, i));
        }

        return result;
    }
}
