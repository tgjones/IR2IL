using System.Reflection.Emit;

namespace IR2IL.ILEmission;

internal sealed class GlobalsILEmitter(CompiledModule compiledModule, TypeBuilder typeBuilder, CompiledGlobalVariable[] globalVariables)
    : ILEmitter(compiledModule, typeBuilder.DefineTypeInitializer().GetILGenerator())
{
    public void EmitGlobalVariablesInitializer()
    {
        foreach (var globalVariable in globalVariables)
        {
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
}