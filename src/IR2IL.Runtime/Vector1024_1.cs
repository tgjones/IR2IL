using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace IR2IL.Runtime;

[StructLayout(LayoutKind.Sequential, Size = Vector1024.Size)]
public readonly struct Vector1024<T>
    where T : unmanaged
{
    internal readonly Vector512<T> _lower;
    internal readonly Vector512<T> _upper;

    public static unsafe int Count => Vector1024.Size / sizeof(T);

    public static Vector32<T> Zero => default;

    public T this[int index] => this.GetElementUnsafe(index);
}