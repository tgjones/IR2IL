using System.Numerics;
using System.Runtime.CompilerServices;

namespace IR2IL.Runtime;

/// <summary>
/// The compare-exchange loops of an LLVM <c>atomicrmw</c>, for the operations that <c>Interlocked</c> has no method for:
/// <c>Add</c> and <c>Subtract</c> on 8 and 16-bit integers, <c>Xor</c>, <c>Nand</c>, <c>Max</c> and <c>Min</c> on integers
/// of 8 to 64 bits, and <c>FAdd</c>, <c>FSub</c>, <c>FMax</c> and <c>FMin</c> on <c>float</c> and <c>double</c>. The emitter
/// calls <c>Interlocked</c> directly for the others. Each loop is instantiated per operation and type so that the JIT
/// inlines the operation. LLVM's signed <c>max</c>/<c>min</c> use a signed integer type and <c>umax</c>/<c>umin</c> an
/// unsigned one.
/// </summary>
public static unsafe class AtomicHelper
{
    public interface IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        static abstract T Apply(T current, T operand);
    }

    public struct Add<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => current + operand;
    }

    public struct Subtract<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => current - operand;
    }

    public struct Nand<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => ~(current & operand);
    }

    public struct Xor<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => current ^ operand;
    }

    public struct Max<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => current > operand ? current : operand;
    }

    public struct Min<T> : IAtomicOperation<T> where T : unmanaged, IBinaryInteger<T>
    {
        public static T Apply(T current, T operand) => current < operand ? current : operand;
    }

    public interface IAtomicFloatOperation<T> where T : unmanaged, IFloatingPointIeee754<T>
    {
        static abstract T Apply(T current, T operand);
    }

    public struct FloatAdd<T> : IAtomicFloatOperation<T> where T : unmanaged, IFloatingPointIeee754<T>
    {
        public static T Apply(T current, T operand) => current + operand;
    }

    public struct FloatSubtract<T> : IAtomicFloatOperation<T> where T : unmanaged, IFloatingPointIeee754<T>
    {
        public static T Apply(T current, T operand) => current - operand;
    }

    /// <summary>LLVM's <c>fmax</c> is <c>llvm.maxnum</c>: a NaN operand gives the other one.</summary>
    public struct FloatMax<T> : IAtomicFloatOperation<T> where T : unmanaged, IFloatingPointIeee754<T>
    {
        public static T Apply(T current, T operand) => T.MaxNumber(current, operand);
    }

    /// <summary>LLVM's <c>fmin</c> is <c>llvm.minnum</c>: a NaN operand gives the other one.</summary>
    public struct FloatMin<T> : IAtomicFloatOperation<T> where T : unmanaged, IFloatingPointIeee754<T>
    {
        public static T Apply(T current, T operand) => T.MinNumber(current, operand);
    }

    /// <summary>Applies <typeparamref name="TOperation"/> to the value at <paramref name="pointer"/>; returns the value it had before.</summary>
    public static T ReadModifyWrite<T, TOperation>(T* pointer, T operand)
        where T : unmanaged, IBinaryInteger<T>
        where TOperation : struct, IAtomicOperation<T>
    {
        var current = *pointer; // May be stale: the compare-exchange checks it.
        while (true)
        {
            var previous = CompareExchange(pointer, TOperation.Apply(current, operand), current);
            if (previous == current)
            {
                return current;
            }

            current = previous;
        }
    }

    /// <summary>
    /// Like <see cref="ReadModifyWrite{T, TOperation}"/> for <c>float</c> and <c>double</c>. The compare-exchange
    /// compares bit patterns, so a NaN in memory ends the loop like any other value.
    /// </summary>
    public static T ReadModifyWriteFloat<T, TOperation>(T* pointer, T operand)
        where T : unmanaged, IFloatingPointIeee754<T>
        where TOperation : struct, IAtomicFloatOperation<T>
    {
        var current = *pointer; // May be stale: the compare-exchange checks it.
        while (true)
        {
            var previous = CompareExchange(pointer, TOperation.Apply(current, operand), current);
            if (SameBits(previous, current))
            {
                return current;
            }

            current = previous;
        }
    }

    /// <summary>
    /// Compares two floating point values by bit pattern, as <see cref="Interlocked.CompareExchange(ref int, int, int)"/>
    /// does, instead of with <c>==</c>. The two disagree for NaN (a NaN never equals itself, so a successful exchange
    /// of a NaN would look like a failure and the operation would be applied again) and for zeros (<c>+0.0 == -0.0</c>,
    /// so a failed exchange that read the other zero would look like a success and the update would be lost).
    /// </summary>
    private static bool SameBits<T>(T a, T b) where T : unmanaged =>
        sizeof(T) == 4 ? Unsafe.BitCast<T, int>(a) == Unsafe.BitCast<T, int>(b) : Unsafe.BitCast<T, long>(a) == Unsafe.BitCast<T, long>(b);

    private static T CompareExchange<T>(T* pointer, T value, T comparand) where T : unmanaged
    {
        if (sizeof(T) == 1)
        {
            return Unsafe.BitCast<byte, T>(Interlocked.CompareExchange(ref *(byte*)pointer, Unsafe.BitCast<T, byte>(value), Unsafe.BitCast<T, byte>(comparand)));
        }

        if (sizeof(T) == 2)
        {
            return Unsafe.BitCast<short, T>(Interlocked.CompareExchange(ref *(short*)pointer, Unsafe.BitCast<T, short>(value), Unsafe.BitCast<T, short>(comparand)));
        }

        if (sizeof(T) == 4)
        {
            return Unsafe.BitCast<int, T>(Interlocked.CompareExchange(ref *(int*)pointer, Unsafe.BitCast<T, int>(value), Unsafe.BitCast<T, int>(comparand)));
        }

        return Unsafe.BitCast<long, T>(Interlocked.CompareExchange(ref *(long*)pointer, Unsafe.BitCast<T, long>(value), Unsafe.BitCast<T, long>(comparand)));
    }
}
