using System.Runtime.CompilerServices;

namespace IR2IL.Runtime;

public static class WideIntegerHelper
{
    public static long Int128ToLong(Int128 value) => (long)value;

    public static Int128 LshrInt128(Int128 value, int shift) => (Int128)((UInt128)value >> shift);

    public static bool UltInt128(Int128 a, Int128 b) => (UInt128)a < (UInt128)b;
    public static bool UleInt128(Int128 a, Int128 b) => (UInt128)a <= (UInt128)b;
    public static bool UgtInt128(Int128 a, Int128 b) => (UInt128)a > (UInt128)b;
    public static bool UgeInt128(Int128 a, Int128 b) => (UInt128)a >= (UInt128)b;

    /// <summary>
    /// Loads <paramref name="byteCount"/> bytes from <paramref name="ptr"/> (little-endian)
    /// into an <see cref="Int128"/>.  Used to materialise LLVM integer types wider than 64 bits
    /// (e.g. i72, i80, i128) that cannot be loaded with a single CLR ldind opcode.
    /// </summary>
    public static unsafe Int128 LoadWideInt(void* ptr, int byteCount)
    {
        if (byteCount == 16)
        {
            return Unsafe.ReadUnaligned<Int128>(ptr);
        }

        ulong lower = 0, upper = 0;

        var lowerBytes = Math.Min(byteCount, 8);
        for (var i = 0; i < lowerBytes; i++)
            lower |= (ulong)((byte*)ptr)[i] << (i * 8);

        for (var i = 8; i < byteCount; i++)
            upper |= (ulong)((byte*)ptr)[i] << ((i - 8) * 8);

        return new Int128(upper, lower);
    }
}
