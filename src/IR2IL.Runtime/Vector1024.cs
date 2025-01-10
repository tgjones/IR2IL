using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

public static class Vector1024
{
    internal const int Size = 128;

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
}
