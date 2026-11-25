using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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

    public readonly struct IntegerMin<T> : IBinaryOperation<T> where T : IBinaryInteger<T> { public static T Apply(T x, T y) => T.Min(x, y); }
    public readonly struct IntegerMax<T> : IBinaryOperation<T> where T : IBinaryInteger<T> { public static T Apply(T x, T y) => T.Max(x, y); }
    public readonly struct PopCount<T> : IUnaryOperation<T> where T : IBinaryInteger<T> { public static T Apply(T x) => T.PopCount(x); }

    /// <summary><c>usub.sat</c>: <c>x - y</c>, or zero when <c>y</c> is larger. Read as unsigned.</summary>
    public readonly struct UnsignedSubtractSaturate<T> : IBinaryOperation<T> where T : IBinaryInteger<T>
    {
        public static T Apply(T x, T y) => x > y ? unchecked(x - y) : T.Zero;
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
}
