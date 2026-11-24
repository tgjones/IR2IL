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

    public static bool IsFPClassF32(float value, int mask)
    {
        // llvm.is.fpclass bitmask: sNaN=0, qNaN=1, -Inf=2, -normal=3, -subnormal=4, -zero=5, +zero=6, +subnormal=7, +normal=8, +Inf=9
        if ((mask & 0x003) != 0 && float.IsNaN(value)) return true;
        if ((mask & 0x004) != 0 && float.IsNegativeInfinity(value)) return true;
        if ((mask & 0x200) != 0 && float.IsPositiveInfinity(value)) return true;
        if ((mask & 0x060) != 0 && value == 0.0f)
        {
            bool isNeg = float.IsNegative(value);
            if ((mask & 0x020) != 0 && isNeg) return true;
            if ((mask & 0x040) != 0 && !isNeg) return true;
        }
        if ((mask & 0x090) != 0 && float.IsSubnormal(value))
        {
            bool isNeg = float.IsNegative(value);
            if ((mask & 0x010) != 0 && isNeg) return true;
            if ((mask & 0x080) != 0 && !isNeg) return true;
        }
        if ((mask & 0x108) != 0 && float.IsNormal(value))
        {
            bool isNeg = float.IsNegative(value);
            if ((mask & 0x008) != 0 && isNeg) return true;
            if ((mask & 0x100) != 0 && !isNeg) return true;
        }
        return false;
    }

    public static bool IsFPClassF64(double value, int mask)
    {
        // llvm.is.fpclass bitmask: sNaN=0, qNaN=1, -Inf=2, -normal=3, -subnormal=4, -zero=5, +zero=6, +subnormal=7, +normal=8, +Inf=9
        if ((mask & 0x003) != 0 && double.IsNaN(value)) return true;
        if ((mask & 0x004) != 0 && double.IsNegativeInfinity(value)) return true;
        if ((mask & 0x200) != 0 && double.IsPositiveInfinity(value)) return true;
        if ((mask & 0x060) != 0 && value == 0.0)
        {
            bool isNeg = double.IsNegative(value);
            if ((mask & 0x020) != 0 && isNeg) return true;
            if ((mask & 0x040) != 0 && !isNeg) return true;
        }
        if ((mask & 0x090) != 0 && double.IsSubnormal(value))
        {
            bool isNeg = double.IsNegative(value);
            if ((mask & 0x010) != 0 && isNeg) return true;
            if ((mask & 0x080) != 0 && !isNeg) return true;
        }
        if ((mask & 0x108) != 0 && double.IsNormal(value))
        {
            bool isNeg = double.IsNegative(value);
            if ((mask & 0x008) != 0 && isNeg) return true;
            if ((mask & 0x100) != 0 && !isNeg) return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int VectorReduceAddV12I32(Vector512<int> vector)
    {
        // v12i32 is stored in a Vector512<int>; only elements 0-11 are valid.
        return Vector256.Sum(vector.GetLower()) + Vector128.Sum(vector.GetUpper().GetLower());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int VectorReduceMulV4I32(Vector128<int> vector)
    {
        if (Sse41.IsSupported)
        {
            // Multiply pairs of elements
            var temp = Sse41.MultiplyLow(vector, Sse41.Shuffle(vector, 0b_10_11_00_01));
            temp = Sse41.MultiplyLow(temp, Sse41.Shuffle(temp, 0b_01_00_11_10));

            // Extract the scalar result
            return Sse41.Extract(temp, 0);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte VectorReduceMulV16I8(Vector128<sbyte> vector)
    {
        // TODO: Implement this method using intrinsics
        var result = (sbyte)1;
        for (var i = 0; i < Vector128<sbyte>.Count; i++)
        {
            result *= vector[i];
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short VectorReduceMulV16I16(Vector256<short> vector)
    {
        // TODO: Implement this method using intrinsics
        var result = (short)1;
        for (var i = 0; i < Vector256<short>.Count; i++)
        {
            result *= vector[i];
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int VectorReduceSMaxV4I32(Vector128<int> vector)
    {
        if (Sse41.IsSupported)
        {
            // Perform horizontal max operations.
            var temp = Sse41.Max(vector, Sse41.Shuffle(vector, 0b_10_11_00_01));
            temp = Sse41.Max(temp, Sse41.Shuffle(temp, 0b_01_00_11_10));
            temp = Sse41.Max(temp, Sse41.Shuffle(temp, 0b_00_01_10_11));

            // Extract the maximum value.
            return Sse2.ConvertToInt32(temp);
        }
        else if (AdvSimd.Arm64.IsSupported)
        {
            // Pairwise max to reduce 4 elements to 1.
            var temp = AdvSimd.Arm64.MaxPairwise(vector, vector);
            temp = AdvSimd.Arm64.MaxPairwise(temp, temp);
            return temp.ToScalar();
        }
        else
        {
            var result = int.MinValue;
            for (var i = 0; i < Vector128<int>.Count; i++)
            {
                if (vector[i] > result)
                {
                    result = vector[i];
                }
            }
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float VectorReduceFAddV4F32(float acc, Vector128<float> vector)
    {
        return acc + Vector128.Sum(vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double VectorReduceFAddV2F64(double acc, Vector128<double> vector)
    {
        return acc + Vector128.Sum(vector);
    }
}