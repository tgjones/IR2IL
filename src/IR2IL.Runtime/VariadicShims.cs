using System.Runtime.InteropServices;

namespace IR2IL.Runtime;

/// <summary>
/// The known variadic functions of the C library. A compiled program calls a variadic function with a hidden last
/// argument: the address of a buffer of 8-byte slots that holds the extra arguments. Each shim takes the fixed
/// arguments and that buffer, and calls the <c>v</c>-version of the function with a <c>va_list</c> built from it.
/// The shims are found by the name of the C function, so every public method of this class is one.
/// </summary>
public static unsafe class VariadicShims
{
    public static int printf(void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.__stdio_common_vfprintf(0, Windows.__acrt_iob_func(1), format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vprintf(format, MakeVaList(va, storage));
    }

    public static int fprintf(void* stream, void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.__stdio_common_vfprintf(0, stream, format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vfprintf(stream, format, MakeVaList(va, storage));
    }

    public static int sprintf(void* buffer, void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            // _CRT_INTERNAL_PRINTF_LEGACY_VSPRINTF_NULL_TERMINATION
            return Windows.__stdio_common_vsprintf(1, buffer, nuint.MaxValue, format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vsprintf(buffer, format, MakeVaList(va, storage));
    }

    public static int snprintf(void* buffer, long size, void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            // _CRT_INTERNAL_PRINTF_STANDARD_SNPRINTF_BEHAVIOR
            return Windows.__stdio_common_vsprintf(2, buffer, (nuint)size, format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vsnprintf(buffer, (nuint)size, format, MakeVaList(va, storage));
    }

    public static int __sprintf_chk(void* buffer, int flag, long bufferSize, void* format, void* va)
    {
        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.__vsprintf_chk(buffer, flag, (nuint)bufferSize, format, MakeVaList(va, storage));
    }

    public static int __snprintf_chk(void* buffer, long size, int flag, long bufferSize, void* format, void* va)
    {
        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.__vsnprintf_chk(buffer, (nuint)size, flag, (nuint)bufferSize, format, MakeVaList(va, storage));
    }

    public static int scanf(void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.__stdio_common_vfscanf(0, Windows.__acrt_iob_func(0), format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vscanf(format, MakeVaList(va, storage));
    }

    public static int fscanf(void* stream, void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.__stdio_common_vfscanf(0, stream, format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vfscanf(stream, format, MakeVaList(va, storage));
    }

    public static int sscanf(void* buffer, void* format, void* va)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.__stdio_common_vsscanf(0, buffer, nuint.MaxValue, format, null, va);
        }

        byte* storage = stackalloc byte[VaListStorageSize];
        return Libc.vsscanf(buffer, format, MakeVaList(va, storage));
    }

    // The size of the largest va_list (AArch64 Linux).
    private const int VaListStorageSize = 32;

    /// <summary>
    /// Builds the <c>va_list</c> argument of a <c>v</c>-function from the buffer of extra arguments, as
    /// <c>va_start</c> does in a compiled function. Where the <c>va_list</c> is a pointer to the next slot (Darwin
    /// ARM64, Windows x64) it is the buffer; elsewhere it is a structure that, however the ABI passes it, the native
    /// function receives as the address of a copy, with all the register arguments marked as used up.
    /// </summary>
    private static void* MakeVaList(void* va, byte* storage)
    {
        var isArm = RuntimeInformation.ProcessArchitecture is Architecture.Arm64;
        if (isArm && (OperatingSystem.IsMacOS() || OperatingSystem.IsIOS()))
        {
            return va;
        }

        new Span<byte>(storage, VaListStorageSize).Clear();
        if (isArm)
        {
            // { void* __stack, void* __gr_top, void* __vr_top, int __gr_offs, int __vr_offs }
            *(void**)storage = va;
        }
        else
        {
            // { uint gp_offset, uint fp_offset, void* overflow_arg_area, void* reg_save_area }
            *(uint*)storage = 48;
            *(uint*)(storage + 4) = 176;
            *(void**)(storage + 8) = va;
        }
        return storage;
    }

    private static class Libc
    {
        private const string Library = "libc";

        [DllImport(Library)] public static extern int vprintf(void* format, void* ap);
        [DllImport(Library)] public static extern int vfprintf(void* stream, void* format, void* ap);
        [DllImport(Library)] public static extern int vsprintf(void* buffer, void* format, void* ap);
        [DllImport(Library)] public static extern int vsnprintf(void* buffer, nuint size, void* format, void* ap);
        [DllImport(Library)] public static extern int __vsprintf_chk(void* buffer, int flag, nuint bufferSize, void* format, void* ap);
        [DllImport(Library)] public static extern int __vsnprintf_chk(void* buffer, nuint size, int flag, nuint bufferSize, void* format, void* ap);
        [DllImport(Library)] public static extern int vscanf(void* format, void* ap);
        [DllImport(Library)] public static extern int vfscanf(void* stream, void* format, void* ap);
        [DllImport(Library)] public static extern int vsscanf(void* buffer, void* format, void* ap);
    }

    /// <summary>
    /// The UCRT exports none of the <c>v</c>-functions: its headers define them over these, with the options
    /// of <c>__local_stdio_printf_options()</c> / <c>__local_stdio_scanf_options()</c>, which are 0 by default.
    /// </summary>
    private static class Windows
    {
        private const string Library = "ucrtbase.dll";

        [DllImport(Library)] public static extern void* __acrt_iob_func(uint index);
        [DllImport(Library)] public static extern int __stdio_common_vfprintf(ulong options, void* stream, void* format, void* locale, void* ap);
        [DllImport(Library)] public static extern int __stdio_common_vsprintf(ulong options, void* buffer, nuint bufferCount, void* format, void* locale, void* ap);
        [DllImport(Library)] public static extern int __stdio_common_vfscanf(ulong options, void* stream, void* format, void* locale, void* ap);
        [DllImport(Library)] public static extern int __stdio_common_vsscanf(ulong options, void* buffer, nuint bufferCount, void* format, void* locale, void* ap);
    }
}
