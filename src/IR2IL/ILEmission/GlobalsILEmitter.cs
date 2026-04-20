using System;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Runtime;

namespace IR2IL.ILEmission;

internal sealed class GlobalsILEmitter(CompiledModule compiledModule, TypeBuilder typeBuilder, CompiledGlobalVariable[] globalVariables)
    : ILEmitter(compiledModule, typeBuilder.DefineTypeInitializer().GetILGenerator())
{
    private static readonly MethodInfo NativeLibraryHelperGetExport =
        typeof(NativeLibraryHelper).GetMethod(nameof(NativeLibraryHelper.GetExport), [typeof(string), typeof(string)])!;

    public void EmitGlobalVariablesInitializer()
    {
        foreach (var globalVariable in globalVariables)
        {
            if (globalVariable.IsExternal)
            {
                EmitExternalGlobalInitializer(globalVariable);
                continue;
            }

            // If the global variable has a zeroinitializer, we don't need to do anything.
            // Static fields are already zero-initialized by the runtime.
            if (globalVariable.Value.Kind == LLVMSharp.Interop.LLVMValueKind.LLVMConstantAggregateZeroValueKind)
            {
                continue;
            }

            EmitConstantValue(globalVariable.Value, globalVariable.Type);
            ILGenerator.Emit(OpCodes.Stsfld, globalVariable.Field);
        }

        ILGenerator.Emit(OpCodes.Ret);
    }

    private void EmitExternalGlobalInitializer(CompiledGlobalVariable globalVariable)
    {
        string libraryName = OperatingSystem.IsWindows() ? "ucrtbase.dll" : "libc";

        // Strip the \x01_ Mach-O prefix if present (same logic as CreateExternMethod).
        var symbolName = globalVariable.Global.Name;
        if (!OperatingSystem.IsWindows() && symbolName.StartsWith("\x01_", StringComparison.Ordinal))
        {
            symbolName = symbolName[2..];
        }

        ILGenerator.Emit(OpCodes.Ldstr, libraryName);
        ILGenerator.Emit(OpCodes.Ldstr, symbolName);
        ILGenerator.Emit(OpCodes.Call, NativeLibraryHelperGetExport);
        ILGenerator.Emit(OpCodes.Stsfld, globalVariable.Field);
    }
}