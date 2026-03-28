using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL;

internal sealed class CompiledModule
{
    private readonly TypeSystem _typeSystem;
    private readonly TypeBuilder _typeBuilder;

    private readonly Dictionary<LLVMValueRef, MethodInfo> _functionLookup = [];
    private readonly Dictionary<LLVMValueRef, FieldInfo> _globalLookup = [];
    private readonly Dictionary<string, MethodBuilder> _printfOverloads = [];

    private static readonly MethodInfo PrintfCore =
        typeof(PrintfHelper).GetMethodStrict(nameof(PrintfHelper.PrintfCore), [typeof(IntPtr), typeof(object[])]);

    public CompiledModule(
        TypeSystem typeSystem,
        TypeBuilder typeBuilder,
        ReadOnlySpan<CompiledGlobalVariable> globalVariables,
        ReadOnlySpan<CompiledFunction> functions)
    {
        _typeSystem = typeSystem;
        _typeBuilder = typeBuilder;

        foreach (var globalVariable in globalVariables)
        {
            _globalLookup.Add(globalVariable.Global, globalVariable.Field);
        }

        foreach (var function in functions)
        {
            _functionLookup.Add(function.Function, function.MethodInfo);
        }
    }

    public TypeSystem TypeSystem => _typeSystem;

    public MethodInfo GetFunction(LLVMValueRef function) => _functionLookup[function];

    public FieldInfo GetGlobal(LLVMValueRef global) => _globalLookup[global];

    // Generates (or retrieves a cached) printf overload for the given parameter types.
    // allParamTypes[0] is the format string (void*); the rest are the vararg argument types.
    public MethodBuilder GetOrCreatePrintfOverload(Type[] allParamTypes)
    {
        var key = string.Join(",", allParamTypes.Select(t => t.FullName));
        if (_printfOverloads.TryGetValue(key, out var cached))
            return cached;

        var method = _typeBuilder.DefineMethod(
            "__printf",
            MethodAttributes.Static | MethodAttributes.Private,
            typeof(int),
            allParamTypes);

        var il = method.GetILGenerator();

        // Load fmt (void*) and convert to IntPtr for PrintfCore
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Conv_I);

        // Build object[] from the vararg arguments (everything after the fmt ptr)
        var varArgCount = allParamTypes.Length - 1;
        il.Emit(OpCodes.Ldc_I4, varArgCount);
        il.Emit(OpCodes.Newarr, typeof(object));

        for (var i = 0; i < varArgCount; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);

            var argIndex = i + 1;
            if (argIndex <= 3)
            {
                il.Emit(argIndex switch
                {
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    _ => OpCodes.Ldarg_3,
                });
            }
            else
            {
                il.Emit(OpCodes.Ldarg_S, (byte)argIndex);
            }

            var argType = allParamTypes[argIndex];
            if (argType.IsPointer)
            {
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Box, typeof(IntPtr));
            }
            else if (argType.IsValueType)
            {
                il.Emit(OpCodes.Box, argType);
            }

            il.Emit(OpCodes.Stelem_Ref);
        }

        il.Emit(OpCodes.Call, PrintfCore);
        il.Emit(OpCodes.Ret);

        _printfOverloads[key] = method;
        return method;
    }
}

internal sealed record CompiledGlobalVariable(LLVMValueRef Global, LLVMTypeRef Type, LLVMValueRef Value, FieldInfo Field);

internal record CompiledFunction(LLVMValueRef Function, MethodInfo MethodInfo);

internal sealed record CompiledFunctionDefinition(LLVMValueRef Function, MethodBuilder MethodBuilder)
    : CompiledFunction(Function, MethodBuilder);
