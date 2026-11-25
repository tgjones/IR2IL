using System.Runtime.InteropServices;

namespace IR2IL.Runtime;

/// <summary>
/// The memory of dynamic <c>alloca</c>s (a variable-length array, or an alloca with a count that isn't a constant).
/// IL has <c>localloc</c>, but it can only be released when the method returns, and <c>llvm.stackrestore</c> has to
/// release memory in the middle of one (a VLA in a loop). So each thread has an arena that is allocated from by
/// bumping a pointer: <c>llvm.stacksave</c> reads the pointer, <c>llvm.stackrestore</c> sets it back, and a function
/// that allocates restores the pointer it found on entry before it returns.
/// </summary>
/// <remarks>
/// The arena is reserved once per thread and the operating system commits its pages as they are touched, so the
/// size is only a limit, like the size of a real stack. It is not freed when the thread ends.
/// </remarks>
public static unsafe class DynamicStack
{
    private const nuint ArenaSize = 256 * 1024 * 1024;

    [ThreadStatic]
    private static byte* t_base;

    [ThreadStatic]
    private static byte* t_top;

    /// <summary><c>llvm.stacksave</c>: the current top of the arena.</summary>
    public static void* Save()
    {
        if (t_base == null)
        {
            Initialize();
        }

        return t_top;
    }

    /// <summary><c>llvm.stackrestore</c>: releases everything allocated since the <see cref="Save"/> that returned <paramref name="top"/>.</summary>
    public static void Restore(void* top) => t_top = (byte*)top;

    /// <summary>Allocates <paramref name="size"/> bytes aligned to <paramref name="alignment"/> (a power of two).</summary>
    public static void* Allocate(nuint size, int alignment)
    {
        if (t_base == null)
        {
            Initialize();
        }

        var mask = (nuint)Math.Max(alignment, 16) - 1;
        var start = (byte*)(((nuint)t_top + mask) & ~mask);
        var end = t_base + ArenaSize;
        if (start > end || size > (nuint)(end - start))
        {
            throw new InsufficientExecutionStackException("The memory for dynamic allocas is exhausted.");
        }

        t_top = start + size;
        return start;
    }

    private static void Initialize()
    {
        // Aligned to a page, so any alignment up to a page holds for the first allocation too.
        t_base = (byte*)NativeMemory.AlignedAlloc(ArenaSize, 4096);
        t_top = t_base;
    }
}
