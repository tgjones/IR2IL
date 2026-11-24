using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace IR2IL.Runtime;

public static class VectorUtility
{
    // CompareNotEquals
    // There is no .NET cross-platform API for e.g. Vector128.NotEquals, so we simulate it.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> CompareVector64NotEquals<T>(Vector64<T> left, Vector64<T> right)
    {
        return Vector64.OnesComplement(Vector64.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CompareVector128NotEquals<T>(Vector128<T> left, Vector128<T> right)
    {
        return Vector128.OnesComplement(Vector128.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> CompareVector256NotEquals<T>(Vector256<T> left, Vector256<T> right)
    {
        return Vector256.OnesComplement(Vector256.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> CompareVector512NotEquals<T>(Vector512<T> left, Vector512<T> right)
    {
        return Vector512.OnesComplement(Vector512.Equals(left, right));
    }

    // IntToPtr

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<nint> IntToPtrV2I64ToV2Ptr(Vector128<long> vector)
    {
        return Vector128.Create(
            Vector64.Create((nint)vector[0]),
            Vector64.Create((nint)vector[1]));
    }

    // Log10

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> Log10V2F32(Vector64<float> vector)
    {
        return Vector64.Log2(vector) * (1f / MathF.Log2(10f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Log10V2F64(Vector128<double> vector)
    {
        return Vector128.Log2(vector) * (1.0 / Math.Log2(10.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Log10V4F32(Vector128<float> vector)
    {
        return Vector128.Log2(vector) * (1f / MathF.Log2(10f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Log10V4F64(Vector256<double> vector)
    {
        return Vector256.Log2(vector) * (1.0 / Math.Log2(10.0));
    }

    // Pow

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> PowV2F32(Vector64<float> left, Vector64<float> right)
    {
        return Vector64.Create(MathF.Pow(left[0], right[0]), MathF.Pow(left[1], right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> PowV2F64(Vector128<double> left, Vector128<double> right)
    {
        return Vector128.Create(Math.Pow(left[0], right[0]), Math.Pow(left[1], right[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> PowV4F32(Vector128<float> left, Vector128<float> right)
    {
        return Vector128.Create(MathF.Pow(left[0], right[0]), MathF.Pow(left[1], right[1]), MathF.Pow(left[2], right[2]), MathF.Pow(left[3], right[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> PowV4F64(Vector256<double> left, Vector256<double> right)
    {
        return Vector256.Create(Math.Pow(left[0], right[0]), Math.Pow(left[1], right[1]), Math.Pow(left[2], right[2]), Math.Pow(left[3], right[3]));
    }

    // Acos

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AcosV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Acos(vector[0]), MathF.Acos(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AcosV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Acos(vector[0]), Math.Acos(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AcosV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Acos(vector[0]), MathF.Acos(vector[1]), MathF.Acos(vector[2]), MathF.Acos(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AcosV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Acos(vector[0]), Math.Acos(vector[1]), Math.Acos(vector[2]), Math.Acos(vector[3]));
    }

    // Asin

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AsinV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Asin(vector[0]), MathF.Asin(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AsinV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Asin(vector[0]), Math.Asin(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AsinV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Asin(vector[0]), MathF.Asin(vector[1]), MathF.Asin(vector[2]), MathF.Asin(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AsinV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Asin(vector[0]), Math.Asin(vector[1]), Math.Asin(vector[2]), Math.Asin(vector[3]));
    }

    // Atan

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> AtanV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Atan(vector[0]), MathF.Atan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> AtanV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Atan(vector[0]), Math.Atan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> AtanV4F32(Vector128<float> vector)
    {
        return Vector128.Create(MathF.Atan(vector[0]), MathF.Atan(vector[1]), MathF.Atan(vector[2]), MathF.Atan(vector[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> AtanV4F64(Vector256<double> vector)
    {
        return Vector256.Create(Math.Atan(vector[0]), Math.Atan(vector[1]), Math.Atan(vector[2]), Math.Atan(vector[3]));
    }

    // Tan

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> TanV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Tan(vector[0]), MathF.Tan(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> TanV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Tan(vector[0]), Math.Tan(vector[1]));
    }

    // Sinh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> SinhV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Sinh(vector[0]), MathF.Sinh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> SinhV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Sinh(vector[0]), Math.Sinh(vector[1]));
    }

    // Cosh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> CoshV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Cosh(vector[0]), MathF.Cosh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> CoshV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Cosh(vector[0]), Math.Cosh(vector[1]));
    }

    // Tanh

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> TanhV2F32(Vector64<float> vector)
    {
        return Vector64.Create(MathF.Tanh(vector[0]), MathF.Tanh(vector[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> TanhV2F64(Vector128<double> vector)
    {
        return Vector128.Create(Math.Tanh(vector[0]), Math.Tanh(vector[1]));
    }

    // Atan2

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> Atan2V2F32(Vector64<float> y, Vector64<float> x)
    {
        return Vector64.Create(MathF.Atan2(y[0], x[0]), MathF.Atan2(y[1], x[1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Atan2V4F32(Vector128<float> y, Vector128<float> x)
    {
        return Vector128.Create(MathF.Atan2(y[0], x[0]), MathF.Atan2(y[1], x[1]), MathF.Atan2(y[2], x[2]), MathF.Atan2(y[3], x[3]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Atan2V2F64(Vector128<double> y, Vector128<double> x)
    {
        return Vector128.Create(Math.Atan2(y[0], x[0]), Math.Atan2(y[1], x[1]));
    }

    // Remainder
    // `srem`, `urem` and `frem` of vectors. There is no SIMD division or remainder, so each lane is computed on its
    // own; one generic method serves every vector type and lane type. Only the real lanes are computed: the
    // padding lanes of a vector may hold anything, such as a zero divisor.

    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector Remainder<TVector, TLane>(TVector left, TVector right, int laneCount)
        where TVector : struct
        where TLane : struct, IModulusOperators<TLane, TLane, TLane>
    {
        var result = default(TVector);
        ref var l = ref Unsafe.As<TVector, TLane>(ref left);
        ref var r = ref Unsafe.As<TVector, TLane>(ref right);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref l, i) % Unsafe.Add(ref r, i);
        }

        return result;
    }

    // ConvertInt64ToSingle, ConvertUInt64ToSingle
    // `sitofp` and `uitofp` of 64-bit lanes to float. Converting to double first would round twice. AVX-512 DQ has
    // one rounding conversion for 2, 4 and 8 lanes; ARM64 has none (its vector conversion doesn't narrow), and nor
    // do other x64 CPUs, so there each lane is converted on its own, which is what LLVM does.

    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult ConvertInt64ToSingle<TVector, TResult>(TVector value, int laneCount)
        where TVector : struct
        where TResult : struct
    {
        if (Avx512DQ.VL.IsSupported)
        {
            // The result of 2 lanes is a Vector64<float>, the lower half of the Vector128<float> of the instruction.
            if (Unsafe.SizeOf<TVector>() == 16 && Unsafe.SizeOf<TResult>() == 8)
            {
                return Unsafe.BitCast<Vector64<float>, TResult>(
                    Avx512DQ.VL.ConvertToVector128Single(Unsafe.BitCast<TVector, Vector128<long>>(value)).GetLower());
            }

            if (Unsafe.SizeOf<TVector>() == 32 && Unsafe.SizeOf<TResult>() == 16)
            {
                return Unsafe.BitCast<Vector128<float>, TResult>(
                    Avx512DQ.VL.ConvertToVector128Single(Unsafe.BitCast<TVector, Vector256<long>>(value)));
            }
        }

        if (Avx512DQ.IsSupported && Unsafe.SizeOf<TVector>() == 64 && Unsafe.SizeOf<TResult>() == 32)
        {
            return Unsafe.BitCast<Vector256<float>, TResult>(
                Avx512DQ.ConvertToVector256Single(Unsafe.BitCast<TVector, Vector512<long>>(value)));
        }

        var result = default(TResult);
        ref var v = ref Unsafe.As<TVector, long>(ref value);
        ref var o = ref Unsafe.As<TResult, float>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref v, i);
        }

        return result;
    }

    /// <inheritdoc cref="ConvertInt64ToSingle{TVector, TResult}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult ConvertUInt64ToSingle<TVector, TResult>(TVector value, int laneCount)
        where TVector : struct
        where TResult : struct
    {
        if (Avx512DQ.VL.IsSupported)
        {
            // The result of 2 lanes is a Vector64<float>, the lower half of the Vector128<float> of the instruction.
            if (Unsafe.SizeOf<TVector>() == 16 && Unsafe.SizeOf<TResult>() == 8)
            {
                return Unsafe.BitCast<Vector64<float>, TResult>(
                    Avx512DQ.VL.ConvertToVector128Single(Unsafe.BitCast<TVector, Vector128<ulong>>(value)).GetLower());
            }

            if (Unsafe.SizeOf<TVector>() == 32 && Unsafe.SizeOf<TResult>() == 16)
            {
                return Unsafe.BitCast<Vector128<float>, TResult>(
                    Avx512DQ.VL.ConvertToVector128Single(Unsafe.BitCast<TVector, Vector256<ulong>>(value)));
            }
        }

        if (Avx512DQ.IsSupported && Unsafe.SizeOf<TVector>() == 64 && Unsafe.SizeOf<TResult>() == 32)
        {
            return Unsafe.BitCast<Vector256<float>, TResult>(
                Avx512DQ.ConvertToVector256Single(Unsafe.BitCast<TVector, Vector512<ulong>>(value)));
        }

        var result = default(TResult);
        ref var v = ref Unsafe.As<TVector, ulong>(ref value);
        ref var o = ref Unsafe.As<TResult, float>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref v, i);
        }

        return result;
    }

    // VariableShift*
    // `shl`, `ashr` and `lshr` of vectors by a vector of counts that are not all the same. A count of the lane
    // width or more is poison, so any result will do. As for Remainder, one generic method serves every vector type
    // and lane type, and only the real lanes are computed.

    /// <param name="laneCount">The number of lanes of the LLVM vector (not of <typeparamref name="TVector"/>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector VariableShiftLeft<TVector, TLane>(TVector value, TVector count, int laneCount)
        where TVector : struct
        where TLane : struct, IBinaryInteger<TLane>
    {
        if (TryShiftWithHardware<TVector, TLane>(LeftShift, value, count, out var hardwareResult))
        {
            return hardwareResult;
        }

        var result = default(TVector);
        ref var v = ref Unsafe.As<TVector, TLane>(ref value);
        ref var c = ref Unsafe.As<TVector, TLane>(ref count);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref v, i) << int.CreateTruncating(Unsafe.Add(ref c, i));
        }

        return result;
    }

    /// <inheritdoc cref="VariableShiftLeft{TVector, TLane}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector VariableShiftRightArithmetic<TVector, TLane>(TVector value, TVector count, int laneCount)
        where TVector : struct
        where TLane : struct, IBinaryInteger<TLane>
    {
        if (TryShiftWithHardware<TVector, TLane>(ArithmeticRightShift, value, count, out var hardwareResult))
        {
            return hardwareResult;
        }

        var result = default(TVector);
        ref var v = ref Unsafe.As<TVector, TLane>(ref value);
        ref var c = ref Unsafe.As<TVector, TLane>(ref count);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref v, i) >> int.CreateTruncating(Unsafe.Add(ref c, i));
        }

        return result;
    }

    /// <inheritdoc cref="VariableShiftLeft{TVector, TLane}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TVector VariableShiftRightLogical<TVector, TLane>(TVector value, TVector count, int laneCount)
        where TVector : struct
        where TLane : struct, IBinaryInteger<TLane>
    {
        if (TryShiftWithHardware<TVector, TLane>(LogicalRightShift, value, count, out var hardwareResult))
        {
            return hardwareResult;
        }

        var result = default(TVector);
        ref var v = ref Unsafe.As<TVector, TLane>(ref value);
        ref var c = ref Unsafe.As<TVector, TLane>(ref count);
        ref var o = ref Unsafe.As<TVector, TLane>(ref result);
        for (var i = 0; i < laneCount; i++)
        {
            Unsafe.Add(ref o, i) = Unsafe.Add(ref v, i) >>> int.CreateTruncating(Unsafe.Add(ref c, i));
        }

        return result;
    }

    private const int LeftShift = 0;
    private const int ArithmeticRightShift = 1;
    private const int LogicalRightShift = 2;

    /// <summary>
    /// Shifts every lane by the count in the same lane of <paramref name="count"/> with a SIMD instruction, when the
    /// CPU has one for this vector and lane type. ARM64 shifts all of them (<c>USHL</c> and <c>SSHL</c> take signed
    /// counts, so a right shift negates the count). AVX2 shifts <c>int</c> and <c>long</c> lanes of 128 and 256 bits,
    /// and AVX-512 adds the arithmetic shift of <c>long</c> lanes. Anything else is left to the caller's loop.
    /// A count of the lane width or more is poison, so the different results of the instructions don't matter.
    /// The padding lanes of a vector may hold anything and are shifted too, which is harmless.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryShiftWithHardware<TVector, TLane>(int kind, TVector value, TVector count, out TVector result)
        where TVector : struct
        where TLane : struct
    {
        if (AdvSimd.IsSupported)
        {
            if (Unsafe.SizeOf<TVector>() == 16)
            {
                if (typeof(TLane) == typeof(sbyte))
                {
                    result = Unsafe.BitCast<Vector128<sbyte>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector128<sbyte>>(value), Unsafe.BitCast<TVector, Vector128<sbyte>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(short))
                {
                    result = Unsafe.BitCast<Vector128<short>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector128<short>>(value), Unsafe.BitCast<TVector, Vector128<short>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(int))
                {
                    result = Unsafe.BitCast<Vector128<int>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector128<int>>(value), Unsafe.BitCast<TVector, Vector128<int>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(long))
                {
                    result = Unsafe.BitCast<Vector128<long>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector128<long>>(value), Unsafe.BitCast<TVector, Vector128<long>>(count)));
                    return true;
                }
            }
            else if (Unsafe.SizeOf<TVector>() == 8)
            {
                if (typeof(TLane) == typeof(sbyte))
                {
                    result = Unsafe.BitCast<Vector64<sbyte>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector64<sbyte>>(value), Unsafe.BitCast<TVector, Vector64<sbyte>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(short))
                {
                    result = Unsafe.BitCast<Vector64<short>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector64<short>>(value), Unsafe.BitCast<TVector, Vector64<short>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(int))
                {
                    result = Unsafe.BitCast<Vector64<int>, TVector>(ShiftArm(kind, Unsafe.BitCast<TVector, Vector64<int>>(value), Unsafe.BitCast<TVector, Vector64<int>>(count)));
                    return true;
                }
            }
        }
        else if (Avx2.IsSupported)
        {
            if (Unsafe.SizeOf<TVector>() == 16)
            {
                if (typeof(TLane) == typeof(int))
                {
                    result = Unsafe.BitCast<Vector128<int>, TVector>(ShiftX86(kind, Unsafe.BitCast<TVector, Vector128<int>>(value), Unsafe.BitCast<TVector, Vector128<int>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(long) && (kind != ArithmeticRightShift || Avx512F.VL.IsSupported))
                {
                    result = Unsafe.BitCast<Vector128<long>, TVector>(ShiftX86(kind, Unsafe.BitCast<TVector, Vector128<long>>(value), Unsafe.BitCast<TVector, Vector128<long>>(count)));
                    return true;
                }
            }
            else if (Unsafe.SizeOf<TVector>() == 32)
            {
                if (typeof(TLane) == typeof(int))
                {
                    result = Unsafe.BitCast<Vector256<int>, TVector>(ShiftX86(kind, Unsafe.BitCast<TVector, Vector256<int>>(value), Unsafe.BitCast<TVector, Vector256<int>>(count)));
                    return true;
                }

                if (typeof(TLane) == typeof(long) && (kind != ArithmeticRightShift || Avx512F.VL.IsSupported))
                {
                    result = Unsafe.BitCast<Vector256<long>, TVector>(ShiftX86(kind, Unsafe.BitCast<TVector, Vector256<long>>(value), Unsafe.BitCast<TVector, Vector256<long>>(count)));
                    return true;
                }
            }
        }

        result = default;
        return false;
    }

    // ARM64: USHL/SSHL shift left by a signed count, and right when it is negative.
    private static Vector128<sbyte> ShiftArm(int kind, Vector128<sbyte> v, Vector128<sbyte> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsByte(), -c).AsSByte(),
    };

    private static Vector128<short> ShiftArm(int kind, Vector128<short> v, Vector128<short> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsUInt16(), -c).AsInt16(),
    };

    private static Vector128<int> ShiftArm(int kind, Vector128<int> v, Vector128<int> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsUInt32(), -c).AsInt32(),
    };

    private static Vector128<long> ShiftArm(int kind, Vector128<long> v, Vector128<long> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsUInt64(), -c).AsInt64(),
    };

    private static Vector64<sbyte> ShiftArm(int kind, Vector64<sbyte> v, Vector64<sbyte> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsByte(), -c).AsSByte(),
    };

    private static Vector64<short> ShiftArm(int kind, Vector64<short> v, Vector64<short> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsUInt16(), -c).AsInt16(),
    };

    private static Vector64<int> ShiftArm(int kind, Vector64<int> v, Vector64<int> c) => kind switch
    {
        LeftShift => AdvSimd.ShiftLogical(v, c),
        ArithmeticRightShift => AdvSimd.ShiftArithmetic(v, -c),
        _ => AdvSimd.ShiftLogical(v.AsUInt32(), -c).AsInt32(),
    };

    // x64: AVX2 has per-lane shifts of 32- and 64-bit lanes (a count above the width gives 0). The arithmetic shift of
    // 64-bit lanes needs AVX-512 (checked by the caller).
    private static Vector128<int> ShiftX86(int kind, Vector128<int> v, Vector128<int> c) => kind switch
    {
        LeftShift => Avx2.ShiftLeftLogicalVariable(v, c.AsUInt32()),
        ArithmeticRightShift => Avx2.ShiftRightArithmeticVariable(v, c.AsUInt32()),
        _ => Avx2.ShiftRightLogicalVariable(v, c.AsUInt32()),
    };

    private static Vector128<long> ShiftX86(int kind, Vector128<long> v, Vector128<long> c) => kind switch
    {
        LeftShift => Avx2.ShiftLeftLogicalVariable(v, c.AsUInt64()),
        ArithmeticRightShift => Avx512F.VL.ShiftRightArithmeticVariable(v, c.AsUInt64()),
        _ => Avx2.ShiftRightLogicalVariable(v, c.AsUInt64()),
    };

    private static Vector256<int> ShiftX86(int kind, Vector256<int> v, Vector256<int> c) => kind switch
    {
        LeftShift => Avx2.ShiftLeftLogicalVariable(v, c.AsUInt32()),
        ArithmeticRightShift => Avx2.ShiftRightArithmeticVariable(v, c.AsUInt32()),
        _ => Avx2.ShiftRightLogicalVariable(v, c.AsUInt32()),
    };

    private static Vector256<long> ShiftX86(int kind, Vector256<long> v, Vector256<long> c) => kind switch
    {
        LeftShift => Avx2.ShiftLeftLogicalVariable(v, c.AsUInt64()),
        ArithmeticRightShift => Avx512F.VL.ShiftRightArithmeticVariable(v, c.AsUInt64()),
        _ => Avx2.ShiftRightLogicalVariable(v, c.AsUInt64()),
    };

    // MaskFromBits: bitcast iN to <N x i1>. Lane i is all ones (-1) when bit i is set, 0 otherwise,
    // the same representation as the result of a vector comparison.

    // The masks of 2 and 4 lanes are held in a Vector64<sbyte> with 8 lanes, like the one of 8 lanes.
    // The lanes after the real ones are undefined, so they can come from the bits above them.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits2(ulong bits) => MaskFromBits8(bits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits4(ulong bits) => MaskFromBits8(bits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> MaskFromBits8(ulong bits) =>
        Vector64.Create(
            MaskLane(bits, 0), MaskLane(bits, 1), MaskLane(bits, 2), MaskLane(bits, 3),
            MaskLane(bits, 4), MaskLane(bits, 5), MaskLane(bits, 6), MaskLane(bits, 7));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> MaskFromBits16(ulong bits) =>
        Vector128.Create(MaskFromBits8(bits), MaskFromBits8(bits >> 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> MaskFromBits32(ulong bits) =>
        Vector256.Create(MaskFromBits16(bits), MaskFromBits16(bits >> 16));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<sbyte> MaskFromBits64(ulong bits) =>
        Vector512.Create(MaskFromBits32(bits), MaskFromBits32(bits >> 32));

    private static sbyte MaskLane(ulong bits, int index) => (sbyte)-(int)((bits >> index) & 1);

    // NormalizeI1Lanes

    // An <N x i1> mask is stored as sbyte lanes, and a true lane is all ones (-1). zext reads only the low bit,
    // so before it is extended (see FunctionILEmitter.EmitVectorLaneConversion) it clears the rest.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> NormalizeI1Lanes(Vector64<sbyte> vector) => vector & Vector64<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> NormalizeI1Lanes(Vector128<sbyte> vector) => vector & Vector128<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> NormalizeI1Lanes(Vector256<sbyte> vector) => vector & Vector256<sbyte>.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<sbyte> NormalizeI1Lanes(Vector512<sbyte> vector) => vector & Vector512<sbyte>.One;

    // Scmp / Ucmp

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Scmp(int a, int b) => a < b ? -1 : a > b ? 1 : 0;
}