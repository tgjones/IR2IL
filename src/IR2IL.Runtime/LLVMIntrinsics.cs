using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IR2IL.Runtime;

public static unsafe class LLVMIntrinsics
{
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
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
}