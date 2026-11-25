using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace IR2IL.Runtime;

public static unsafe class LLVMIntrinsics
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreUnordered(double a, double b)
    {
        return double.IsNaN(a) || double.IsNaN(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreUnordered(float a, float b)
    {
        return float.IsNaN(a) || float.IsNaN(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreOrderedAndNotEqual(double a, double b)
    {
        return a > b || a < b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreOrderedAndNotEqual(float a, float b)
    {
        return a > b || a < b;
    }

    /// <summary>
    /// <c>llvm.experimental.vector.partial.reduce.add.v4i32.v16i32</c>: adds the lanes of <paramref name="vector"/> into
    /// the lanes of <paramref name="accumulator"/>. LLVM leaves the grouping of the lanes open, so lane <c>i</c>
    /// takes the lanes <c>i</c>, <c>i + 4</c>, <c>i + 8</c> and <c>i + 12</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> VectorPartialReduceAddV4I32V16I32(Vector128<int> accumulator, Vector512<int> vector)
    {
        var half = vector.GetLower() + vector.GetUpper();
        return accumulator + half.GetLower() + half.GetUpper();
    }

    /// <summary>
    /// <c>llvm.experimental.cttz.elts.i64.v2i1</c>: the number of lanes before the first one that is true, or 2 when none
    /// is. The mask is a <c>Vector64&lt;sbyte&gt;</c> whose lanes after the real two are undefined, and a lane is true when
    /// its low bit is set. When <paramref name="isZeroPoison"/> is set, a mask with no true lane is poison, so 2 is fine.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CttzElemsV2I1(Vector64<sbyte> mask, bool isZeroPoison)
    {
        return (mask[0] & 1) != 0 ? 0 : (mask[1] & 1) != 0 ? 1 : 2;
    }

    /// <summary>
    /// <c>llvm.experimental.memset.pattern</c>: stores <paramref name="count"/> copies of <paramref name="pattern"/>
    /// from <paramref name="destination"/>. <paramref name="count"/> is a number of elements, not of bytes.
    /// </summary>
    public static void MemSetPattern<T>(void* destination, T pattern, long count)
        where T : unmanaged
    {
        new Span<T>(destination, checked((int)count)).Fill(pattern);
    }

    // llvm.is.fpclass test bits: sNaN=0x1, qNaN=0x2, -Inf=0x4, -normal=0x8, -subnormal=0x10, -zero=0x20,
    // +zero=0x40, +subnormal=0x80, +normal=0x100, +Inf=0x200. A NaN is quiet when the top bit of its mantissa is set.

    public static bool IsFPClassF32(float value, int mask) =>
        IsFPClass(value, mask, (BitConverter.SingleToInt32Bits(value) & 0x00400000) != 0);

    public static bool IsFPClassF64(double value, int mask) =>
        IsFPClass(value, mask, (BitConverter.DoubleToInt64Bits(value) & 0x0008000000000000) != 0);

    private static bool IsFPClass<T>(T value, int mask, bool isQuietNaN)
        where T : IFloatingPointIeee754<T>
    {
        var negative = T.IsNegative(value);
        int cls;
        if (T.IsNaN(value)) cls = isQuietNaN ? 0x2 : 0x1;
        else if (T.IsInfinity(value)) cls = negative ? 0x4 : 0x200;
        else if (T.IsZero(value)) cls = negative ? 0x20 : 0x40;
        else if (T.IsSubnormal(value)) cls = negative ? 0x10 : 0x80;
        else cls = negative ? 0x8 : 0x100;
        return (mask & cls) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int VectorReduceAddV12I32(Vector512<int> vector)
    {
        // v12i32 is stored in a Vector512<int>; only elements 0-11 are valid.
        return Vector256.Sum(vector.GetLower()) + Vector128.Sum(vector.GetUpper().GetLower());
    }
}