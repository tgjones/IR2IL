using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace IR2IL.Runtime;

[DebuggerDisplay("{DisplayString,nq}")]
[DebuggerTypeProxy(typeof(Vector32DebugView<>))]
[StructLayout(LayoutKind.Sequential, Size = Vector32.Size)]
public readonly struct Vector32<T>
    where T : unmanaged
{
    public static unsafe int Count => Vector32.Size / sizeof(T);

    public static Vector32<T> Zero => default;

    public T this[int index] => this.GetElementUnsafe(index);

    internal string DisplayString => ToString();

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append('<');
        sb.Append(((IFormattable)this.GetElementUnsafe(0)).ToString());

        var separator = NumberFormatInfo.GetInstance(CultureInfo.InvariantCulture).NumberGroupSeparator;

        for (int i = 1; i < Count; i++)
        {
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(((IFormattable)this.GetElementUnsafe(i)).ToString());
        }
        sb.Append('>');

        return sb.ToString();
    }
}