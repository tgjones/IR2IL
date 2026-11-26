using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using IR2IL.Helpers;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL;

internal sealed class CompiledModule
{
    private readonly TypeSystem _typeSystem;
    private readonly TypeBuilder _typeBuilder;

    private readonly Dictionary<LLVMValueRef, MethodInfo> _functionLookup = [];
    private readonly Dictionary<LLVMValueRef, (FieldInfo Field, bool IsExternal)> _globalLookup = [];
    private MethodBuilder? _inlineArrayElementRef;

    private static readonly MethodInfo UnsafeAsOpenMethod =
        typeof(Unsafe).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Unsafe.As) && m.GetGenericArguments().Length == 2);

    private static readonly MethodInfo UnsafeAddIntOpenMethod =
        typeof(Unsafe).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Unsafe.Add)
                      && m.GetGenericArguments().Length == 1
                      && m.GetParameters().Length == 2
                      && m.GetParameters()[0].ParameterType.IsByRef
                      && m.GetParameters()[1].ParameterType == typeof(int));

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
            _globalLookup.Add(globalVariable.Global, (globalVariable.Field, globalVariable.IsExternal));
        }

        foreach (var function in functions)
        {
            _functionLookup.Add(function.Function, function.MethodInfo);
        }
    }

    public TypeSystem TypeSystem => _typeSystem;

    public MethodInfo GetFunction(LLVMValueRef function) => _functionLookup[function];

    public (FieldInfo Field, bool IsExternal) GetGlobal(LLVMValueRef global) => _globalLookup[global];

    // Generates (or retrieves) a generic helper equivalent to Roslyn's:
    //   static ref TElement InlineArrayElementRef<TBuffer, TElement>(ref TBuffer buffer, int index)
    //       => ref Unsafe.Add(ref Unsafe.As<TBuffer, TElement>(ref buffer), index);
    public MethodBuilder GetOrCreateInlineArrayElementRef()
    {
        if (_inlineArrayElementRef is not null)
            return _inlineArrayElementRef;

        var method = _typeBuilder.DefineMethod(
            "InlineArrayElementRef",
            MethodAttributes.Static | MethodAttributes.Assembly);

        var genericParams = method.DefineGenericParameters("TBuffer", "TElement");
        var tBuffer = genericParams[0];
        var tElement = genericParams[1];

        method.SetReturnType(tElement.MakeByRefType());
        method.SetParameters(tBuffer.MakeByRefType(), typeof(int));

        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, UnsafeAsOpenMethod.MakeGenericMethod(tBuffer, tElement));
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, UnsafeAddIntOpenMethod.MakeGenericMethod(tElement));
        il.Emit(OpCodes.Ret);

        _inlineArrayElementRef = method;
        return method;
    }
}

internal sealed record CompiledGlobalVariable(LLVMValueRef Global, LLVMTypeRef Type, LLVMValueRef Value, FieldInfo Field, bool IsExternal = false);

internal record CompiledFunction(LLVMValueRef Function, MethodInfo MethodInfo);

internal sealed record CompiledFunctionDefinition(LLVMValueRef Function, MethodBuilder MethodBuilder)
    : CompiledFunction(Function, MethodBuilder);
