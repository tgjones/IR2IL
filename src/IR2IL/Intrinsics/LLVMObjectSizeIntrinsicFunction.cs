using System.Reflection.Emit;
using LLVMSharp.Interop;

namespace IR2IL.Intrinsics;

/// <summary>
/// <c>llvm.objectsize.iN.p0(ptr, i1 min, i1 nullunknown, i1 dynamic)</c>: the number of bytes from the pointer to the
/// end of the object it points into, evaluated at compile time like LLVM's own lowering (which clang runs even at
/// <c>-O0</c>). It follows constant <c>getelementptr</c>s back to an <c>alloca</c> with a constant count or to a
/// global variable that can't be replaced at link time. For anything else the size is unknown: -1, or 0 when
/// <c>min</c> is set.
/// </summary>
internal sealed class LLVMObjectSizeIntrinsicFunction(int resultBits) : IntrinsicFunction
{
    public override bool IsPure => true;

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        var min = context.Operands[1].ConstIntZExt != 0;
        var size = TryGetSize(context.Operands[0], min, context) ?? (min ? 0L : -1L);
        if (resultBits == 64)
        {
            context.ILGenerator.Emit(OpCodes.Ldc_I8, size);
        }
        else
        {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, unchecked((int)size));
        }
    }

    private static long? TryGetSize(LLVMValueRef pointer, bool min, IntrinsicFunctionCallContext context)
    {
        var offset = 0L;
        while (IsGetElementPtr(pointer))
        {
            for (var i = 1u; i < pointer.OperandCount; i++)
            {
                if (pointer.GetOperand(i).Kind != LLVMValueKind.LLVMConstantIntValueKind)
                {
                    return null;
                }
            }

            offset += context.TypeSystem.GetConstantElementPtrOffset(pointer);
            pointer = pointer.GetOperand(0);
        }

        long size;
        if (pointer.IsAAllocaInst != null && pointer.GetOperand(0).Kind == LLVMValueKind.LLVMConstantIntValueKind)
        {
            size = context.TypeSystem.GetAllocSizeOfTypeInBytes(pointer.GetAllocatedType()) * pointer.GetOperand(0).ConstIntSExt;
        }
        else if (pointer.IsAGlobalVariable != null && HasKnownSize(pointer, min))
        {
            size = context.TypeSystem.GetAllocSizeOfTypeInBytes(pointer.GlobalValueType);
        }
        else
        {
            return null;
        }

        // A pointer past the end of the object has no bytes left; one before its start is not evaluated.
        return offset < 0 ? null : offset > size ? 0 : size - offset;
    }

    private static bool IsGetElementPtr(LLVMValueRef value) =>
        value.IsAGetElementPtrInst != null
        || (value.Kind == LLVMValueKind.LLVMConstantExprValueKind && value.ConstOpcode == LLVMOpcode.LLVMGetElementPtr);

    // The size of a global that the linker may replace with another one is only a lower bound: it is known for `min`.
    private static bool HasKnownSize(LLVMValueRef global, bool min) =>
        !global.IsDeclaration
        && (min || global.Linkage is LLVMLinkage.LLVMExternalLinkage or LLVMLinkage.LLVMInternalLinkage or LLVMLinkage.LLVMPrivateLinkage
            or LLVMLinkage.LLVMWeakODRLinkage or LLVMLinkage.LLVMLinkOnceODRLinkage);
}
