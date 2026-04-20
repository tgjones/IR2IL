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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector32<short> VectorShiftRightLogical(Vector32<short> vector, Vector32<short> count)
    {
        return Vector32.Create(
            (short)((ushort)vector[0] >> count[0]),
            (short)((ushort)vector[1] >> count[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> VectorShiftRightLogical(Vector64<int> vector, Vector64<int> count)
    {
        if (Avx2.IsSupported)
        {
            return Avx2.ShiftRightLogicalVariable(vector.ToVector128(), count.AsUInt32().ToVector128()).GetLower();
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> VectorShiftRightLogical(Vector128<int> vector, Vector128<int> count)
    {
        if (Avx2.IsSupported)
        {
            return Avx2.ShiftRightLogicalVariable(vector, count.AsUInt32());
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
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