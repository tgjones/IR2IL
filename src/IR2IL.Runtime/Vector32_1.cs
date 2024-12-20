using System.Runtime.InteropServices;

namespace IR2IL.Runtime;

[StructLayout(LayoutKind.Sequential, Size = Vector32.Size)]
public readonly struct Vector32<T>
    where T : unmanaged
{
    public static unsafe int Count => Vector32.Size / sizeof(T);

    public static Vector32<T> Zero => default;

    public T this[int index] => this.GetElementUnsafe(index);
}