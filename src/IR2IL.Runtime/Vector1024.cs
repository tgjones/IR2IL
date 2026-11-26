using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

public static class Vector1024
{
    internal const int Size = 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Create<T>(Vector512<T> lower, Vector512<T> upper)
        where T : unmanaged
    {
        Unsafe.SkipInit(out Vector1024<T> result);
        Unsafe.AsRef(in result._lower) = lower;
        Unsafe.AsRef(in result._upper) = upper;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Add<T>(Vector1024<T> left, Vector1024<T> right)
        where T : unmanaged, INumber<T>
    {
        return Create(
            Vector512.Add(left.GetLower(), right.GetLower()),
            Vector512.Add(left.GetUpper(), right.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetElement<T>(this Vector1024<T> vector, int index)
        where T : unmanaged
    {
        if ((uint)index >= (uint)Vector1024<T>.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return vector.GetElementUnsafe(index);
    }

    public static Vector512<T> GetLower<T>(this Vector1024<T> vector)
        where T : unmanaged
    {
        return vector._lower;
    }

    public static Vector512<T> GetUpper<T>(this Vector1024<T> vector)
        where T : unmanaged
    {
        return vector._upper;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(this Vector1024<T> vector)
        where T : unmanaged, IAdditionOperators<T, T, T>
    {
        return Vector512.Sum(vector.GetLower()) + Vector512.Sum(vector.GetUpper());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> WithElement<T>(this Vector1024<T> vector, int index, T value)
        where T : unmanaged
    {
        Vector1024<T> result = vector;
        result.SetElementUnsafe(index, value);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T GetElementUnsafe<T>(in this Vector1024<T> vector, int index)
        where T : unmanaged
    {
        Debug.Assert((index >= 0) && (index < Vector1024<T>.Count));
        ref T address = ref Unsafe.As<Vector1024<T>, T>(ref Unsafe.AsRef(in vector));
        return Unsafe.Add(ref address, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetElementUnsafe<T>(in this Vector1024<T> vector, int index, T value)
        where T : unmanaged
    {
        Debug.Assert((index >= 0) && (index < Vector1024<T>.Count));
        ref T address = ref Unsafe.As<Vector1024<T>, T>(ref Unsafe.AsRef(in vector));
        Unsafe.Add(ref address, index) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Sin(Vector1024<float> x)
    {
        return Create(Vector512.Sin(x.GetLower()), Vector512.Sin(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Cos(Vector1024<float> x)
    {
        return Create(Vector512.Cos(x.GetLower()), Vector512.Cos(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Asin(Vector1024<float> x)
    {
        return Create(Vector512.Asin(x.GetLower()), Vector512.Asin(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Exp(Vector1024<float> x)
    {
        return Create(Vector512.Exp(x.GetLower()), Vector512.Exp(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Log(Vector1024<float> x)
    {
        return Create(Vector512.Log(x.GetLower()), Vector512.Log(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> Ceiling(Vector1024<float> x)
    {
        return Create(Vector512.Ceiling(x.GetLower()), Vector512.Ceiling(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<float> FusedMultiplyAdd(Vector1024<float> left, Vector1024<float> right, Vector1024<float> addend)
    {
        return Create(Vector512.FusedMultiplyAdd(left.GetLower(), right.GetLower(), addend.GetLower()), Vector512.FusedMultiplyAdd(left.GetUpper(), right.GetUpper(), addend.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Sin(Vector1024<double> x)
    {
        return Create(Vector512.Sin(x.GetLower()), Vector512.Sin(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Cos(Vector1024<double> x)
    {
        return Create(Vector512.Cos(x.GetLower()), Vector512.Cos(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Asin(Vector1024<double> x)
    {
        return Create(Vector512.Asin(x.GetLower()), Vector512.Asin(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Exp(Vector1024<double> x)
    {
        return Create(Vector512.Exp(x.GetLower()), Vector512.Exp(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Log(Vector1024<double> x)
    {
        return Create(Vector512.Log(x.GetLower()), Vector512.Log(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> Ceiling(Vector1024<double> x)
    {
        return Create(Vector512.Ceiling(x.GetLower()), Vector512.Ceiling(x.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<double> FusedMultiplyAdd(Vector1024<double> left, Vector1024<double> right, Vector1024<double> addend)
    {
        return Create(Vector512.FusedMultiplyAdd(left.GetLower(), right.GetLower(), addend.GetLower()), Vector512.FusedMultiplyAdd(left.GetUpper(), right.GetUpper(), addend.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Sqrt<T>(Vector1024<T> vector)
        where T : unmanaged
    {
        return Create(Vector512.Sqrt(vector.GetLower()), Vector512.Sqrt(vector.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Abs<T>(Vector1024<T> vector)
        where T : unmanaged
    {
        return Create(Vector512.Abs(vector.GetLower()), Vector512.Abs(vector.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> CopySign<T>(Vector1024<T> value, Vector1024<T> sign)
        where T : unmanaged
    {
        return Create(Vector512.CopySign(value.GetLower(), sign.GetLower()), Vector512.CopySign(value.GetUpper(), sign.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Min<T>(Vector1024<T> left, Vector1024<T> right)
        where T : unmanaged
    {
        return Create(Vector512.Min(left.GetLower(), right.GetLower()), Vector512.Min(left.GetUpper(), right.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> Max<T>(Vector1024<T> left, Vector1024<T> right)
        where T : unmanaged
    {
        return Create(Vector512.Max(left.GetLower(), right.GetLower()), Vector512.Max(left.GetUpper(), right.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<T> SubtractSaturate<T>(Vector1024<T> left, Vector1024<T> right)
        where T : unmanaged
    {
        return Create(Vector512.SubtractSaturate(left.GetLower(), right.GetLower()), Vector512.SubtractSaturate(left.GetUpper(), right.GetUpper()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1024<TTo> As<TFrom, TTo>(this Vector1024<TFrom> vector)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        return Unsafe.BitCast<Vector1024<TFrom>, Vector1024<TTo>>(vector);
    }
}
