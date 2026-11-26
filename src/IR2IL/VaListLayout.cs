using System;
using System.Reflection.Emit;

namespace IR2IL;

/// <summary>
/// The <c>va_list</c> of the module's target. A variadic function receives its extra arguments in a buffer of
/// 8-byte slots (see <see cref="Intrinsics.LLVMVaStartIntrinsicFunction"/>), and <c>va_start</c> builds the
/// target's <c>va_list</c> so that the code clang generates for <c>va_arg</c> reads every argument from that buffer.
/// </summary>
internal sealed class VaListLayout
{
    private enum Kind
    {
        /// <summary>A pointer to the next slot (Darwin ARM64, Windows x64).</summary>
        Pointer,

        /// <summary><c>{ ptr __stack, ptr __gr_top, ptr __vr_top, i32 __gr_offs, i32 __vr_offs }</c></summary>
        AArch64,

        /// <summary><c>{ i32 gp_offset, i32 fp_offset, ptr overflow_arg_area, ptr reg_save_area }</c></summary>
        X64,
    }

    private readonly Kind _kind;

    private VaListLayout(Kind kind, bool isWindows)
    {
        _kind = kind;
        IsWindows = isWindows;
    }

    public bool IsWindows { get; }

    /// <summary>Whether the <c>va_list</c> is a pointer to the next slot, which is what the <c>va_arg</c> instruction needs.</summary>
    public bool IsPointer => _kind == Kind.Pointer;

    /// <summary>The size of the <c>va_list</c> object in bytes.</summary>
    public int SizeInBytes => _kind switch
    {
        Kind.Pointer => 8,
        Kind.AArch64 => 32,
        _ => 24,
    };

    /// <summary>Whether an argument of 16 bytes (<c>i128</c>, <c>fp128</c>) starts on a 16-byte boundary.</summary>
    public bool Align16ByteArguments => !IsWindows;

    public static VaListLayout FromTriple(string triple)
    {
        var lower = triple.ToLowerInvariant();
        var architecture = lower.Split('-')[0];
        var isWindows = lower.Contains("windows") || lower.Contains("win32") || lower.Contains("mingw");
        var isApple = lower.Contains("apple") || lower.Contains("darwin") || lower.Contains("macos");

        if (isWindows)
        {
            return new VaListLayout(Kind.Pointer, true);
        }

        switch (architecture)
        {
            case "aarch64" or "arm64":
                return new VaListLayout(isApple ? Kind.Pointer : Kind.AArch64, false);
            case "x86_64" or "amd64":
                return new VaListLayout(Kind.X64, false);
            default:
                throw new NotSupportedException($"Variadic functions are not supported for target '{triple}'.");
        }
    }

    /// <summary>
    /// Emits <c>va_start</c>: the <c>va_list</c> whose address is on the stack is initialised so that its next
    /// argument is the first slot of the buffer, with no register arguments left.
    /// </summary>
    public void EmitVaStart(ILGenerator il, Action emitVaBuffer)
    {
        switch (_kind)
        {
            case Kind.Pointer:
                emitVaBuffer();
                il.Emit(OpCodes.Stind_I);
                break;

            case Kind.AArch64:
            {
                var list = il.DeclareLocal(typeof(void*));
                il.Emit(OpCodes.Stloc, list);

                Store(il, list, 0, OpCodes.Stind_I, () => emitVaBuffer());
                Store(il, list, 8, OpCodes.Stind_I, () => EmitNull(il));
                Store(il, list, 16, OpCodes.Stind_I, () => EmitNull(il));
                Store(il, list, 24, OpCodes.Stind_I4, () => il.Emit(OpCodes.Ldc_I4_0));
                Store(il, list, 28, OpCodes.Stind_I4, () => il.Emit(OpCodes.Ldc_I4_0));
                break;
            }

            default:
            {
                var list = il.DeclareLocal(typeof(void*));
                il.Emit(OpCodes.Stloc, list);

                // gp_offset = 48 and fp_offset = 176 mean that all the register slots are used up.
                Store(il, list, 0, OpCodes.Stind_I4, () => il.Emit(OpCodes.Ldc_I4, 48));
                Store(il, list, 4, OpCodes.Stind_I4, () => il.Emit(OpCodes.Ldc_I4, 176));
                Store(il, list, 8, OpCodes.Stind_I, () => emitVaBuffer());
                Store(il, list, 16, OpCodes.Stind_I, () => EmitNull(il));
                break;
            }
        }
    }

    private static void Store(ILGenerator il, LocalBuilder list, int offset, OpCode store, Action emitValue)
    {
        il.Emit(OpCodes.Ldloc, list);
        il.Emit(OpCodes.Ldc_I4, offset);
        il.Emit(OpCodes.Add);
        emitValue();
        il.Emit(store);
    }

    private static void EmitNull(ILGenerator il)
    {
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Conv_U);
    }
}
