using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading;
using IR2IL.Helpers;
using IR2IL.Intrinsics;
using IR2IL.Runtime;
using LLVMSharp.Interop;

namespace IR2IL.ILEmission;

internal sealed class FunctionILEmitter : ILEmitter
{
    private readonly MethodInfo _method;
    private readonly LLVMValueRef _function;

    private readonly Dictionary<LLVMValueRef, bool> CanPushToStackLookup = [];

    private readonly Dictionary<LLVMValueRef, ParameterBuilder> Parameters = [];
    private readonly Dictionary<LLVMValueRef, LocalBuilder> Locals = [];
    private readonly Dictionary<LLVMBasicBlockRef, Label> Labels = [];

    public readonly Dictionary<LLVMValueRef, LocalBuilder> PhiLocals = [];

    private int _previousSequencePointOffset = -1;

    public FunctionILEmitter(
        CompiledModule compiledModule,
        CompiledFunctionDefinition compiledFunction)
        : base(compiledModule, compiledFunction.MethodBuilder.GetILGenerator())
    {
        _method = compiledFunction.MethodBuilder;
        _function = compiledFunction.Function;

        // Figure out which instructions need their results stored in local variables,
        // and which can be pushed to the stack.
        foreach (var basicBlock in _function.BasicBlocks)
        {
            var blockInstructions = basicBlock.GetInstructions().ToList();
            for (var i = 0; i < blockInstructions.Count; i++)
            {
                CanPushToStackLookup.Add(blockInstructions[i], CanPushToStack(blockInstructions, i));
            }
        }

        for (var i = 0; i < _function.Params.Length; i++)
        {
            var parameter = _function.Params[i];
            var parameterIndex = i + 1;

            var parameterName = _function.GetParameterName(parameterIndex);

            var parameterBuilder = compiledFunction.MethodBuilder.DefineParameter(
                parameterIndex,
                ParameterAttributes.None, // TODO
                parameterName);

            Parameters.Add(parameter, parameterBuilder);
        }
    }

    private static bool CanPushToStack(List<LLVMValueRef> instructions, int index)
    {
        var instruction = instructions[index];
        var users = instruction.GetUses().ToList();

        if (users.Count != 1)
        {
            return false;
        }

        var user = users[0];

        // If it's used in a different block from where it's executed,
        // we can't push it to the stack because we can't guarantee the
        // order of execution. (With more effort we perhaps could.)
        if (user.InstructionParent != instruction.InstructionParent)
        {
            return false;
        }

        // We can never inline an alloca instruction.
        if (instruction.InstructionOpcode == LLVMOpcode.LLVMAlloca)
        {
            return false;
        }

        // If user is next instruction, then we can always inline it,
        // regardless of the current or next instruction types.
        if (user == instruction.NextInstruction)
        {
            return true;
        }

        if (instruction.InstructionOpcode == LLVMOpcode.LLVMLoad)
        {
            // Make sure the result of this load instruction is used
            // before anything that might change its value.
            for (var j = index + 1; j < instructions.Count; j++)
            {
                if (instructions[j] == user)
                {
                    return true;
                }
                if (!instructions[j].HasNoSideEffects())
                {
                    return false;
                }
            }
            throw new InvalidOperationException("Shouldn't be here");
        }
        else if (instruction.HasNoSideEffects())
        {
            return true;
        }
        return false;
    }

    public void Compile()
    {
        foreach (var basicBlock in _function.BasicBlocks)
        {
            foreach (var instruction in basicBlock.GetInstructions())
            {
                switch (instruction.InstructionOpcode)
                {
                    case LLVMOpcode.LLVMPHI:
                        PhiLocals.Add(
                            instruction,
                            ILGenerator.DeclareLocal(TypeSystem.GetMsilType(instruction.TypeOf)));
                        break;
                }
            }
        }

        foreach (var basicBlock in _function.BasicBlocks)
        {
            var basicBlockLabel = GetOrCreateLabel(basicBlock);
            ILGenerator.MarkLabel(basicBlockLabel);

            foreach (var instruction in basicBlock.GetInstructions())
            {
                if (!CanPushToStack(instruction) && instruction.InstructionOpcode != LLVMOpcode.LLVMPHI)
                {
                    CompileInstruction(instruction);
                }
            }
        }
    }

    private Label GetOrCreateLabel(LLVMBasicBlockRef basicBlock)
    {
        if (!Labels.TryGetValue(basicBlock, out var result))
        {
            Labels.Add(basicBlock, result = ILGenerator.DefineLabel());
        }
        return result;
    }

    private bool CanPushToStack(LLVMValueRef valueRef)
    {
        return CanPushToStackLookup.TryGetValue(valueRef, out var value) && value;
    }

    private void CompileInstruction(LLVMValueRef instruction)
    {
        // Check for debug metadata.
        if (instruction.IsADbgInfoIntrinsic == null)
        {
            var debugLoc = instruction.GetDebugLoc();
            if (debugLoc != null && ILGenerator.ILOffset != _previousSequencePointOffset)
            {
                var line = (int)debugLoc.GetDILocationLine();
                var column = (int)debugLoc.GetDILocationColumn();
                var scope = debugLoc.GetDILocationScope();

                var debugFile = scope.GetDIScopeFile();

                var document = TypeSystem.GetDocument(debugFile);

                ILGenerator.MarkSequencePoint(
                   document,
                   line, column,
                   line, column + 1);

                _previousSequencePointOffset = ILGenerator.ILOffset;
            }
        }

        CompileInstructionValue(instruction);

        if (instruction.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind
            && instruction.InstructionOpcode != LLVMOpcode.LLVMAlloca)
        {
            if (instruction.GetUses().Any())
            {
                EmitStoreResult(instruction);
            }
            else
            {
                ILGenerator.Emit(OpCodes.Pop);
            }
        }
    }

    private void CompileInstructionValue(LLVMValueRef instruction)
    {
        if (instruction.Kind != LLVMValueKind.LLVMInstructionValueKind)
        {
            throw new InvalidOperationException();
        }

        switch (instruction.InstructionOpcode)
        {
            case LLVMOpcode.LLVMAdd:
            case LLVMOpcode.LLVMFAdd:
                EmitBinaryOperation(instruction, OpCodes.Add, nameof(Vector128.Add), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMAnd:
                EmitBinaryOperation(instruction, OpCodes.And, nameof(Vector128.BitwiseAnd), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMAlloca:
                EmitAlloca(instruction);
                break;

            case LLVMOpcode.LLVMAShr:
                EmitBinaryOperation(instruction, OpCodes.Shr, nameof(Vector128.ShiftRightArithmetic), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMAtomicRMW:
                EmitAtomicRMW(instruction);
                break;

            case LLVMOpcode.LLVMBitCast:
                EmitBitCast(instruction);
                break;

            case LLVMOpcode.LLVMBr:
                EmitBr(instruction);
                break;

            case LLVMOpcode.LLVMCall:
                EmitCall(instruction);
                break;

            case LLVMOpcode.LLVMExtractElement:
                EmitExtractElement(instruction);
                break;

            case LLVMOpcode.LLVMFreeze:
                EmitFreeze(instruction);
                break;

            case LLVMOpcode.LLVMFCmp:
                EmitFCmp(instruction);
                break;

            case LLVMOpcode.LLVMGetElementPtr:
                EmitGetElementPtr(instruction);
                break;

            case LLVMOpcode.LLVMICmp:
                EmitICmp(instruction);
                break;

            case LLVMOpcode.LLVMInsertElement:
                EmitInsertElement(instruction);
                break;

            case LLVMOpcode.LLVMLoad:
                EmitLoad(instruction);
                break;

            case LLVMOpcode.LLVMFDiv:
                EmitBinaryOperation(instruction, OpCodes.Div, nameof(Vector128.Divide), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMFMul:
                EmitBinaryOperation(instruction, OpCodes.Mul, nameof(Vector128.Multiply), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMFNeg:
                EmitUnaryOperation(instruction, OpCodes.Neg, nameof(Vector128.Negate), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMFPExt:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMFPToSI:
                EmitConversion(instruction, Signedness.Signed);
                break;

            case LLVMOpcode.LLVMFPToUI:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMFPTrunc:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMIntToPtr:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMLShr:
                EmitBinaryOperation(instruction, OpCodes.Shr_Un, nameof(Vector128.ShiftRightLogical), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMMul:
                EmitBinaryOperation(instruction, OpCodes.Mul, nameof(Vector128.Multiply), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMOr:
                EmitBinaryOperation(instruction, OpCodes.Or, nameof(Vector128.BitwiseOr), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMPHI:
                ILGenerator.Emit(OpCodes.Ldloc, PhiLocals[instruction]);
                break;

            case LLVMOpcode.LLVMPtrToInt:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMRet:
                {
                    if (instruction.OperandCount > 0)
                    {
                        var returnOperand = instruction.GetOperand(0);
                        EmitValue(returnOperand);
                    }
                    ILGenerator.Emit(OpCodes.Ret);
                    break;
                }

            case LLVMOpcode.LLVMSDiv:
                EmitBinaryOperation(instruction, OpCodes.Div, nameof(Vector128.Divide), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMSelect:
                EmitSelect(instruction);
                break;

            case LLVMOpcode.LLVMSExt:
                EmitConversion(instruction, Signedness.Signed);
                break;

            case LLVMOpcode.LLVMShl:
                EmitBinaryOperation(instruction, OpCodes.Shl, nameof(Vector128.ShiftLeft), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMStore:
                EmitStore(instruction);
                break;

            case LLVMOpcode.LLVMShuffleVector:
                EmitShuffleVector(instruction);
                break;

            case LLVMOpcode.LLVMSIToFP:
                EmitConversion(instruction, Signedness.Signed);
                break;

            case LLVMOpcode.LLVMSub:
            case LLVMOpcode.LLVMFSub:
                EmitBinaryOperation(instruction, OpCodes.Sub, nameof(Vector128.Subtract), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMSRem:
                EmitBinaryOperation(instruction, OpCodes.Rem, "SignedRemainder", Signedness.Signed);
                break;

            case LLVMOpcode.LLVMSwitch:
                EmitSwitch(instruction);
                break;

            case LLVMOpcode.LLVMTrunc:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMUDiv:
                EmitBinaryOperation(instruction, OpCodes.Div_Un, nameof(Vector128.Divide), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMUIToFP:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMUnreachable:
                ILGenerator.Emit(OpCodes.Ldstr, "Unreachable instruction");
                ILGenerator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructorStrict([typeof(string)]));
                ILGenerator.Emit(OpCodes.Throw);
                break;

            case LLVMOpcode.LLVMURem:
                EmitBinaryOperation(instruction, OpCodes.Rem_Un, "UnsignedRemainder", Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMXor:
                EmitBinaryOperation(instruction, OpCodes.Xor, nameof(Vector128.Xor), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMZExt:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            default:
                throw new NotImplementedException($"Instruction {instruction.InstructionOpcode} is not implemented: {instruction}");
        }
    }

    private void EmitAtomicRMW(LLVMValueRef instruction)
    {
        var pointer = instruction.GetOperand(0);
        var value = instruction.GetOperand(1);

        switch (instruction.AtomicRMWBinOp)
        {
            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd:
                switch (value.TypeOf.Kind)
                {
                    case LLVMTypeKind.LLVMIntegerTypeKind:
                        // TODO: When amount to add is constant 1 or -1, call Increment or Decrement.

                        EmitValue(pointer);
                        EmitValue(value);

                        var integerType = TypeSystem.GetMsilType(value.TypeOf);
                        var addMethod = typeof(Interlocked).GetMethodStrict(nameof(Interlocked.Add), [integerType.MakeByRefType(), integerType]);
                        ILGenerator.Emit(OpCodes.Call, addMethod);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                break;

            default:
                throw new NotImplementedException($"Atomic RMW operation {instruction.AtomicRMWBinOp} not implemented: {instruction}");
        }
    }

    private void EmitBitCast(LLVMValueRef instruction)
    {
        EmitValue(instruction.GetOperand(0));

        var fromType = instruction.GetOperand(0).TypeOf;
        var toType = instruction.TypeOf;

        var fromTypeMsil = TypeSystem.GetMsilType(fromType);
        var toTypeMsil = TypeSystem.GetMsilType(toType);

        switch (fromType.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
            case LLVMTypeKind.LLVMFloatTypeKind:
            case LLVMTypeKind.LLVMDoubleTypeKind:
                var bitCastMethod = typeof(Unsafe).GetStaticMethodStrict(nameof(Unsafe.BitCast)).MakeGenericMethod(
                    fromTypeMsil,
                    toTypeMsil);
                ILGenerator.Emit(OpCodes.Call, bitCastMethod);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                // If fromType is <N x i1>, call ExtractMostSignificantBits first,
                // and set fromType to integer of correct size.
                if (fromType.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                    && fromType.ElementType.IntWidth == 1)
                {
                    var extractMostSignificantBitsMethod = TypeSystem.GetNonGenericVectorType(fromType)
                        .GetStaticMethodStrict(nameof(Vector128.ExtractMostSignificantBits))
                        .MakeGenericMethod(TypeSystem.GetMsilVectorElementType(fromType.ElementType));
                    ILGenerator.Emit(OpCodes.Call, extractMostSignificantBitsMethod);

                    fromTypeMsil = TypeSystem.GetIntegerType(TypeSystem.GetSizeOfTypeInBits(fromType));
                }
                goto case LLVMTypeKind.LLVMIntegerTypeKind;

            default:
                throw new InvalidOperationException($"Unsupported bitcast from type {fromType} to type {toType}: {instruction}");
        }
    }

    private void EmitExtractElement(LLVMValueRef instruction)
    {
        // Vector
        EmitValue(instruction.GetOperand(0));

        // Index
        EmitValue(instruction.GetOperand(1));
        ILGenerator.Emit(OpCodes.Conv_I4);

        var vectorType = instruction.GetOperand(0).TypeOf;
        var getElementMethod = TypeSystem.GetNonGenericVectorType(vectorType)
            .GetStaticMethodStrict(nameof(Vector128.GetElement))
            .MakeGenericMethod(TypeSystem.GetMsilVectorElementType(vectorType.ElementType));
        ILGenerator.Emit(OpCodes.Call, getElementMethod);
    }

    private void EmitAlloca(LLVMValueRef instruction)
    {
        var numElements = instruction.GetOperand(0);

        // TODO: Alignment

        switch (numElements.Kind)
        {
            case LLVMValueKind.LLVMConstantIntValueKind:
                var allocatedType = instruction.GetAllocatedType();
                var localType = numElements.ConstIntSExt != 1
                    ? TypeSystem.GetArrayType(allocatedType, (int)numElements.ConstIntSExt)
                    : TypeSystem.GetMsilType(allocatedType);
                var local = ILGenerator.DeclareLocal(localType);
                Locals.Add(instruction, local);
                break;

            case LLVMValueKind.LLVMInstructionValueKind:
                EmitValue(numElements);
                ILGenerator.Emit(OpCodes.Conv_U);
                ILGenerator.Emit(OpCodes.Localloc);
                EmitStoreResult(instruction);
                break;

            default:
                throw new NotImplementedException($"Alloca not implemented for kind {numElements.Kind}: {instruction}");
        }
    }

    private void EmitFreeze(LLVMValueRef instruction)
    {
        EmitValue(instruction.GetOperand(0));
    }

    private void EmitStore(LLVMValueRef instruction)
    {
        var value = instruction.GetOperand(0);
        var ptr = instruction.GetOperand(1);

        if (ptr.IsAAllocaInst != null && Locals.TryGetValue(ptr, out var local) && (local.LocalType.IsPrimitive || local.LocalType.IsPointer))
        {
            EmitValue(value);
            ILGenerator.Emit(OpCodes.Stloc, local);
        }
        else
        {
            EmitValue(ptr);
            EmitValue(value);
            EmitStoreIndirect(value.TypeOf);
        }
    }

    private void EmitShuffleVector(LLVMValueRef instruction)
    {
        // shufflevector is used for a few distinct purposes.
        // We handle them separately.

        // 1. insertelement and shufflevector are used together to splat a scalar across a whole vector.
        //
        // LLVM:
        // %1 = insertelement <2 x double> poison, double 1.0, i64 0
        // %2 = shufflevector <2 x double> %1, <2 x double> poison, <2 x i32> zeroinitializer
        //
        // .NET:
        // Vector128.Create(1.0)

        var sourceVector0 = instruction.GetOperand(0);
        var sourceVector1 = instruction.GetOperand(1);
        var maskIndices = instruction.GetShuffleVectorMaskValues();

        if (sourceVector0.Kind == LLVMValueKind.LLVMInstructionValueKind
            && sourceVector0.InstructionOpcode == LLVMOpcode.LLVMInsertElement
            && sourceVector0.GetOperand(0).Kind == LLVMValueKind.LLVMPoisonValueValueKind
            && sourceVector0.GetOperand(2).Kind == LLVMValueKind.LLVMConstantIntValueKind
            && sourceVector0.GetOperand(2).ConstIntZExt == 0
            && maskIndices.All(x => x == 0))
        {
            // Emit scalar value.
            var scalarValue = sourceVector0.GetOperand(1);
            EmitValue(scalarValue);

            // Create vector from scalar value.
            var scalarValueType = TypeSystem.GetMsilType(scalarValue.TypeOf);
            ILGenerator.Emit(
                OpCodes.Call,
                TypeSystem.GetNonGenericVectorType(instruction.TypeOf).GetMethodStrict("Create", [scalarValueType]));

            return;
        }

        // 2. Combining two vectors.
        //
        // LLVM:
        // %22 = shufflevector <4 x float> %15, <4 x float> %18, <8 x i32> <i32 0, i32 1, i32 2, i32 3, i32 4, i32 5, i32 6, i32 7>
        //
        // .NET:
        // Vector256.Create(%15, %18)

        if (maskIndices.Length == sourceVector0.TypeOf.VectorSize * 2
            && maskIndices.SequenceEqual(Enumerable.Range(0, maskIndices.Length)))
        {
            // Emit source vectors.
            EmitValue(sourceVector0);
            EmitValue(sourceVector1);

            // Emit call to Create.
            var sourceVectorType = TypeSystem.GetMsilType(sourceVector0.TypeOf);
            ILGenerator.Emit(
                OpCodes.Call,
                TypeSystem.GetNonGenericVectorType(instruction.TypeOf).GetMethodStrict("Create", [sourceVectorType, sourceVectorType]));

            return;
        }

        // 3. A "true" shuffle where we select elements from two input vectors.
        //    For smaller vectors, we could use .NET's shuffle vector APIs and do it in hardware,
        //    and this is a TODO. Although even then there aren't yet shuffle APIs that take two
        //    input vectors; that's is being discussed in https://github.com/dotnet/runtime/issues/63331
        //    But if the input vector is too large to fit into even a Vector512, we need to use
        //    a custom struct and do it in software. For now, for simplicity, we always use a
        //    custom struct.
        //
        // LLVM:
        // %25 = shufflevector <16 x float> %24, <16 x float> <float 2.000000e+00, float 2.000000e+00, float 2.000000e+00, float 2.000000e+00, float 3.000000e+00, float 3.000000e+00, float 3.000000e+00, float 3.000000e+00, float poison, float poison, float poison, float poison, float poison, float poison, float poison, float poison>, <24 x i32> <i32 0, i32 4, i32 8, i32 12, i32 16, i32 20, i32 1, i32 5, i32 9, i32 13, i32 17, i32 21, i32 2, i32 6, i32 10, i32 14, i32 18, i32 22, i32 3, i32 7, i32 11, i32 15, i32 19, i32 23>
        //
        // .NET:
        // StructArray24 result;
        // result[0] = %24[0];
        // result[1] = %24[4];
        // result[2] = %24[8];
        // result[3] = %24[12];
        // result[4] = 2.0;
        // ...

        var sourceVector0Local = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(sourceVector0.TypeOf));
        EmitValue(sourceVector0);
        ILGenerator.Emit(OpCodes.Stloc, sourceVector0Local);

        var sourceVector1Local = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(sourceVector1.TypeOf));
        EmitValue(sourceVector1);
        ILGenerator.Emit(OpCodes.Stloc, sourceVector1Local);

        var elementSizeInBytes = TypeSystem.GetSizeOfTypeInBytes(instruction.TypeOf.ElementType);
        var resultLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(instruction.TypeOf));

        for (var i = 0; i < maskIndices.Length; i++)
        {
            // Emit destination address.
            ILGenerator.Emit(OpCodes.Ldloca, resultLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, i * elementSizeInBytes);
            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);

            // Emit value.
            var maskIndex = maskIndices[i];
            var sourceVectorLocal = maskIndex < sourceVector0.TypeOf.VectorSize
                ? sourceVector0Local
                : sourceVector1Local;
            var sourceVectorIndex = maskIndex < sourceVector0.TypeOf.VectorSize
                ? maskIndex
                : maskIndex - (int)sourceVector0.TypeOf.VectorSize;
            ILGenerator.Emit(OpCodes.Ldloca, sourceVectorLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, sourceVectorIndex * elementSizeInBytes);
            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);
            EmitLoadIndirect(instruction.TypeOf.ElementType);

            // Emit store indirect instruction.
            EmitStoreIndirect(instruction.TypeOf.ElementType);
        }

        // Load the result.
        ILGenerator.Emit(OpCodes.Ldloc, resultLocal);
    }

    private void EmitInsertElement(LLVMValueRef instruction)
    {
        // Vector
        var vectorOperand = instruction.GetOperand(0);
        EmitValue(vectorOperand);

        // Index
        EmitValue(instruction.GetOperand(2));
        ILGenerator.Emit(OpCodes.Conv_I4);

        // Value
        var valueOperand = instruction.GetOperand(1);
        EmitValue(valueOperand);

        EmitVectorWithElement(
            TypeSystem.GetNonGenericVectorType(vectorOperand.TypeOf),
            TypeSystem.GetMsilType(valueOperand.TypeOf));
    }

    private void EmitVectorWithElement(Type nonGenericVectorType, Type valueType)
    {
        var withElementMethod = nonGenericVectorType
            .GetStaticMethodStrict(nameof(Vector128.WithElement))
            .MakeGenericMethod(valueType);
        ILGenerator.Emit(OpCodes.Call, withElementMethod);
    }

    private void EmitICmp(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);

        EmitValue(operand0);
        EmitValue(instruction.GetOperand(1));

        switch (operand0.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
            case LLVMTypeKind.LLVMPointerTypeKind:
                switch (instruction.ICmpPredicate)
                {
                    case LLVMIntPredicate.LLVMIntEQ:
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntNE:
                        ILGenerator.Emit(OpCodes.Ceq);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntSGE:
                        ILGenerator.Emit(OpCodes.Clt);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntSGT:
                        ILGenerator.Emit(OpCodes.Cgt);
                        break;

                    case LLVMIntPredicate.LLVMIntSLE:
                        ILGenerator.Emit(OpCodes.Cgt);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntSLT:
                        ILGenerator.Emit(OpCodes.Clt);
                        break;

                    case LLVMIntPredicate.LLVMIntUGE:
                        ILGenerator.Emit(OpCodes.Clt_Un);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntUGT:
                        ILGenerator.Emit(OpCodes.Cgt_Un);
                        break;

                    case LLVMIntPredicate.LLVMIntULT:
                        ILGenerator.Emit(OpCodes.Clt_Un);
                        break;

                    default:
                        throw new NotImplementedException($"Integer comparison predicate {instruction.ICmpPredicate} not implemented: {instruction}");
                }
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(instruction.GetOperand(0).TypeOf);
                var (vectorUtilityType, vectorComparisonMethodName) = instruction.ICmpPredicate switch
                {
                    LLVMIntPredicate.LLVMIntEQ => (nonGenericVectorType, nameof(Vector128.Equals)),
                    LLVMIntPredicate.LLVMIntNE => (typeof(VectorUtility), $"Compare{nonGenericVectorType.Name}NotEquals"),
                    LLVMIntPredicate.LLVMIntSGE => (nonGenericVectorType, nameof(Vector128.GreaterThanOrEqual)),
                    LLVMIntPredicate.LLVMIntSGT => (nonGenericVectorType, nameof(Vector128.GreaterThan)),
                    LLVMIntPredicate.LLVMIntSLT => (nonGenericVectorType, nameof(Vector128.LessThan)),
                    LLVMIntPredicate.LLVMIntUGT => (nonGenericVectorType, nameof(Vector128.GreaterThan)),
                    LLVMIntPredicate.LLVMIntULT => (nonGenericVectorType, nameof(Vector128.LessThan)),
                    _ => throw new NotImplementedException($"Integer comparison predicate {instruction.ICmpPredicate} not implemented for vectors: {instruction}"),
                };
                EmitVectorComparison(instruction, vectorUtilityType, vectorComparisonMethodName);
                break;

            default:
                throw new NotImplementedException($"ICmp not implemented for type {operand0.TypeOf.Kind}: {instruction}");
        }
    }

    private void EmitVectorComparison(LLVMValueRef instruction, Type vectorUtilityType, string vectorComparisonMethodName)
    {
        var operand0 = instruction.GetOperand(0);

        var genericVectorMethod = vectorUtilityType.GetStaticMethodStrict(vectorComparisonMethodName);
        var elementType = TypeSystem.GetMsilVectorElementType(operand0.TypeOf.ElementType);
        var vectorMethod = genericVectorMethod.MakeGenericMethod(elementType);
        ILGenerator.Emit(OpCodes.Call, vectorMethod);

        // If result is not an integer type, bitcast it to integer type.
        var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(operand0.TypeOf);
        switch (operand0.TypeOf.ElementType.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsInt64)).MakeGenericMethod(elementType));
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsInt32)).MakeGenericMethod(elementType));
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                // Nothing to do.
                break;

            default:
                throw new NotImplementedException($"Vector comparison not implemented for element type {operand0.TypeOf.ElementType.Kind}: {instruction}");
        }

        // Save intermediate result to local, since we'll need to reference it multiple times below.
        var integerVectorType = TypeSystem.GetGenericVectorType(operand0.TypeOf).MakeGenericType(TypeSystem.GetIntegerType(TypeSystem.GetSizeOfTypeInBits(operand0.TypeOf.ElementType)));
        var intermediateLocal = ILGenerator.DeclareLocal(integerVectorType);
        ILGenerator.Emit(OpCodes.Stloc, intermediateLocal);

        // We need to truncate the result from e.g. Vector128<long> to Vector16<sbyte>,
        // something like this:
        // Vector16<sbyte>.Zero.WithElement(0, (sbyte)input[0]).WithElement(1, (sbyte)input[1]);
        var resultVectorType = TypeSystem.GetMsilVectorType(instruction.TypeOf);
        ILGenerator.Emit(OpCodes.Call, resultVectorType.GetMethodStrict("get_Zero"));

        var inputVectorType = TypeSystem.GetMsilVectorType(operand0.TypeOf);
        for (var i = 0; i < instruction.TypeOf.VectorSize; i++)
        {
            ILGenerator.Emit(OpCodes.Ldc_I4, i);
            ILGenerator.Emit(OpCodes.Ldloca, intermediateLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, i);
            ILGenerator.Emit(OpCodes.Call, integerVectorType.GetMethodStrict("get_Item"));

            // Cast to `sbyte`.
            ILGenerator.Emit(OpCodes.Conv_I1);

            EmitVectorWithElement(
                TypeSystem.GetNonGenericVectorType(instruction.TypeOf),
                typeof(sbyte));
        }
    }

    private void EmitFCmp(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);

        EmitValue(operand0);
        EmitValue(instruction.GetOperand(1));

        switch (operand0.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
            case LLVMTypeKind.LLVMFloatTypeKind:
                switch (instruction.FCmpPredicate)
                {
                    case LLVMRealPredicate.LLVMRealOEQ:
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMRealPredicate.LLVMRealOGE:
                        ILGenerator.Emit(OpCodes.Clt);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMRealPredicate.LLVMRealOGT:
                        ILGenerator.Emit(OpCodes.Cgt);
                        break;

                    case LLVMRealPredicate.LLVMRealOLE:
                        ILGenerator.Emit(OpCodes.Cgt);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMRealPredicate.LLVMRealOLT:
                        ILGenerator.Emit(OpCodes.Clt);
                        break;

                    case LLVMRealPredicate.LLVMRealUGE:
                        ILGenerator.Emit(OpCodes.Clt_Un);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMRealPredicate.LLVMRealULE:
                        ILGenerator.Emit(OpCodes.Cgt_Un);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMRealPredicate.LLVMRealULT:
                        ILGenerator.Emit(OpCodes.Clt_Un);
                        break;

                    case LLVMRealPredicate.LLVMRealUNE:
                        ILGenerator.Emit(OpCodes.Ceq);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    default:
                        throw new NotImplementedException($"Float comparison predicate {instruction.FCmpPredicate} not implemented: {instruction}");
                }
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(instruction.GetOperand(0).TypeOf);
                var (vectorUtilityType, vectorComparisonMethodName) = instruction.FCmpPredicate switch
                {
                    LLVMRealPredicate.LLVMRealOEQ => (nonGenericVectorType, nameof(Vector128.Equals)),
                    LLVMRealPredicate.LLVMRealOGT => (nonGenericVectorType, nameof(Vector128.GreaterThan)),
                    LLVMRealPredicate.LLVMRealOLT => (nonGenericVectorType, nameof(Vector128.LessThan)),
                    LLVMRealPredicate.LLVMRealUGE => (nonGenericVectorType, nameof(Vector128.GreaterThanOrEqual)),
                    LLVMRealPredicate.LLVMRealUNE => (typeof(VectorUtility), $"Compare{nonGenericVectorType.Name}NotEquals"),
                    _ => throw new NotImplementedException($"Float comparison predicate {instruction.FCmpPredicate} not implemented for vectors: {instruction}"),
                };
                EmitVectorComparison(instruction, vectorUtilityType, vectorComparisonMethodName);
                break;

            default:
                throw new NotImplementedException($"FCmp not implemented for type {operand0.TypeOf.Kind}: {instruction}");
        }

        // TODO: Do the right thing for ordered / unordered equality comparisons.
    }

    private enum Signedness
    {
        Signed,
        Unsigned,
    }

    private void EmitConversion(LLVMValueRef instruction, Signedness signedness)
    {
        EmitConversion(
            instruction.InstructionOpcode,
            instruction.GetOperand(0),
            instruction.TypeOf,
            signedness);
    }

    private void EmitConversion(LLVMOpcode opcode, LLVMValueRef operand, LLVMTypeRef toType, Signedness signedness)
    {
        var fromType = operand.TypeOf;
        EmitValue(operand);

        switch (toType.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                // Ensure source type is treated as signed.
                if (opcode == LLVMOpcode.LLVMSIToFP)
                {
                    switch (fromType.IntWidth)
                    {
                        case 8:
                            ILGenerator.Emit(OpCodes.Conv_I1);
                            break;

                        case 16:
                            ILGenerator.Emit(OpCodes.Conv_I2);
                            break;
                    }
                }
                if (opcode == LLVMOpcode.LLVMUIToFP)
                {
                    switch (fromType.IntWidth)
                    {
                        case 8:
                            ILGenerator.Emit(OpCodes.Conv_U1);
                            break;

                        case 16:
                            ILGenerator.Emit(OpCodes.Conv_U2);
                            break;
                    }
                    ILGenerator.Emit(OpCodes.Conv_R_Un);
                }
                ILGenerator.Emit(OpCodes.Conv_R8);
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                // Ensure source type is treated as signed.
                if (opcode == LLVMOpcode.LLVMSIToFP)
                {
                    switch (fromType.IntWidth)
                    {
                        case 8:
                            ILGenerator.Emit(OpCodes.Conv_I1);
                            break;

                        case 16:
                            ILGenerator.Emit(OpCodes.Conv_I2);
                            break;
                    }
                }
                if (opcode == LLVMOpcode.LLVMUIToFP)
                {
                    switch (fromType.IntWidth)
                    {
                        case 8:
                            ILGenerator.Emit(OpCodes.Conv_U1);
                            break;

                        case 16:
                            ILGenerator.Emit(OpCodes.Conv_U2);
                            break;
                    }
                    ILGenerator.Emit(OpCodes.Conv_R_Un);
                }
                ILGenerator.Emit(OpCodes.Conv_R4);
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                switch (opcode)
                {
                    case LLVMOpcode.LLVMSExt:
                        // Ensure source type is treated as signed.
                        if (fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        {
                            switch (fromType.IntWidth)
                            {
                                case 8:
                                    ILGenerator.Emit(OpCodes.Conv_I1);
                                    break;

                                case 16:
                                    ILGenerator.Emit(OpCodes.Conv_I2);
                                    break;
                            }
                        }
                        break;

                    case LLVMOpcode.LLVMZExt:
                        // Ensure source type is treated as unsigned.
                        if (fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        {
                            switch (fromType.IntWidth)
                            {
                                case 8:
                                    ILGenerator.Emit(OpCodes.Conv_U1);
                                    break;

                                case 16:
                                    ILGenerator.Emit(OpCodes.Conv_U2);
                                    break;
                            }
                        }
                        break;
                }

                EmitIntegerConversion((int)toType.IntWidth, signedness);
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                ILGenerator.Emit(OpCodes.Conv_I);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                var convertMethodPrefix = opcode.ToString().Remove(0, 4);
                var convertMethodName = $"{convertMethodPrefix}{GetIntrinsicMethodSuffix(fromType)}To{GetIntrinsicMethodSuffix(toType)}";
                var convertMethod = typeof(VectorUtility).GetStaticMethodStrict(convertMethodName);
                ILGenerator.Emit(OpCodes.Call, convertMethod);
                break;

            default:
                throw new NotImplementedException($"Conversion not implemented to {toType} for operand {operand}: {opcode}");
        }
    }

    private void EmitIntegerConversion(int bits, Signedness signedness)
    {
        var roundedUpBits = TypeSystem.RoundUpToTypeSize(bits);

        switch (roundedUpBits, signedness)
        {
            case (8, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I1);
                break;

            case (8, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U1);
                break;

            case (16, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I2);
                break;

            case (16, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U2);
                break;

            case (32, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I4);
                break;

            case (32, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U4);
                break;

            case (64, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I8);
                break;

            case (64, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U8);
                break;

            default:
                throw new NotImplementedException($"Integer conversion not implemented to int width {bits} {signedness}");
        }

        if (bits < roundedUpBits)
        {
            EmitConstantIntegerValue((uint)bits, -1);
            ILGenerator.Emit(OpCodes.And);
        }
    }

    private void EmitUnaryOrBinaryOperation(
        LLVMValueRef instruction,
        OpCode scalarOpCode,
        string vectorMethodName,
        int operandCount,
        Signedness signedness)
    {
        if (instruction.OperandCount != operandCount)
        {
            throw new InvalidOperationException();
        }

        var isVectorShiftWithVectorCount = false;

        for (var i = 0u; i < operandCount; i++)
        {
            var operand = instruction.GetOperand(i);

            if (instruction.TypeOf.Kind == LLVMTypeKind.LLVMVectorTypeKind)
            {
                switch (vectorMethodName)
                {
                    case nameof(Vector128.ShiftLeft):
                    case nameof(Vector128.ShiftRightArithmetic):
                    case nameof(Vector128.ShiftRightLogical):
                        if (i == 1)
                        {
                            // Amount to shift by. We currently only support constant values,
                            // where all values in the vector are the same constant.
                            switch (operand.Kind)
                            {
                                case LLVMValueKind.LLVMConstantDataVectorValueKind:
                                    var firstValue = operand.GetAggregateElement(0);
                                    var firstValueConstant = firstValue.Kind switch
                                    {
                                        LLVMValueKind.LLVMConstantIntValueKind => (int)firstValue.ConstIntSExt,
                                        _ => throw new NotImplementedException(),
                                    };
                                    for (var j = 1u; j < operand.TypeOf.VectorSize; j++)
                                    {
                                        var value = operand.GetAggregateElement(j);
                                        if (firstValueConstant != (int)value.ConstIntSExt)
                                        {
                                            isVectorShiftWithVectorCount = true;
                                            break;
                                        }
                                    }
                                    if (isVectorShiftWithVectorCount)
                                    {
                                        EmitValue(operand);
                                    }
                                    else
                                    {
                                        ILGenerator.Emit(OpCodes.Ldc_I4, firstValueConstant);
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    default:
                        EmitValue(operand);
                        break;
                }
            }
            else
            {
                if (i == 0 && instruction.InstructionOpcode == LLVMOpcode.LLVMAShr)
                {
                    EmitConversion(LLVMOpcode.LLVMSExt, operand, instruction.TypeOf, Signedness.Signed);
                }
                else
                {
                    EmitValue(operand);

                    if (operand.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                    {
                        EmitIntegerConversion((int)operand.TypeOf.IntWidth, signedness);
                    }
                }
            }
        }

        switch (instruction.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
            case LLVMTypeKind.LLVMFloatTypeKind:
            case LLVMTypeKind.LLVMIntegerTypeKind:
                ILGenerator.Emit(scalarOpCode);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                if (vectorMethodName == null)
                {
                    throw new NotImplementedException();
                }
                var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(instruction.TypeOf);
                MethodInfo vectorMethod;
                switch (vectorMethodName)
                {
                    case nameof(Vector128.ShiftLeft):
                    case nameof(Vector128.ShiftRightArithmetic):
                    case nameof(Vector128.ShiftRightLogical):
                        var vectorType = TypeSystem.GetMsilType(instruction.TypeOf);
                        if (isVectorShiftWithVectorCount)
                        {
                            vectorMethod = typeof(LLVMIntrinsics).GetMethodStrict($"Vector{vectorMethodName}", [vectorType, vectorType]);
                        }
                        else
                        {
                            vectorMethod = nonGenericVectorType.GetMethodStrict(vectorMethodName, [vectorType, typeof(int)]);
                        }
                        break;

                    case "SignedRemainder":
                    case "UnsignedRemainder":
                        vectorMethod = typeof(VectorUtility).GetStaticMethodStrict($"{vectorMethodName}{GetIntrinsicMethodSuffix(instruction.TypeOf)}");
                        break;

                    default:
                        var genericVectorType = TypeSystem.GetGenericVectorType(instruction.TypeOf).MakeGenericType(Type.MakeGenericMethodParameter(0));
                        var genericVectorMethod = nonGenericVectorType.GetMethodStrict(vectorMethodName, Enumerable.Repeat(genericVectorType, operandCount).ToArray()); ;
                        var elementType = TypeSystem.GetMsilVectorElementType(instruction.TypeOf.ElementType);
                        vectorMethod = genericVectorMethod.MakeGenericMethod(elementType);
                        break;
                }
                ILGenerator.EmitCall(OpCodes.Call, vectorMethod, null);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private static string GetIntrinsicMethodSuffix(LLVMTypeRef typeRef)
    {
        var result = new StringBuilder();

        if (typeRef.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            result.Append('V');
            result.Append(typeRef.VectorSize);
        }

        var elementType = typeRef.Kind == LLVMTypeKind.LLVMVectorTypeKind
            ? typeRef.ElementType
            : typeRef;

        result.Append(elementType.Kind switch
        {
            LLVMTypeKind.LLVMIntegerTypeKind => $"I{TypeSystem.RoundUpToTypeSize((int)elementType.IntWidth)}",
            LLVMTypeKind.LLVMDoubleTypeKind => "F64",
            LLVMTypeKind.LLVMFloatTypeKind => "F32",
            LLVMTypeKind.LLVMPointerTypeKind => "Ptr",
            _ => throw new InvalidOperationException($"Unsupported type {elementType}")
        });

        return result.ToString();
    }

    private void EmitUnaryOperation(
        LLVMValueRef instruction,
        OpCode scalarOpCode,
        string vectorMethodName,
        Signedness signedness)
    {
        EmitUnaryOrBinaryOperation(instruction, scalarOpCode, vectorMethodName, 1, signedness);
    }

    private void EmitBinaryOperation(
        LLVMValueRef instruction,
        OpCode scalarOpCode,
        string vectorMethodName,
        Signedness signedness)
    {
        EmitUnaryOrBinaryOperation(instruction, scalarOpCode, vectorMethodName, 2, signedness);
    }

    private void EmitBr(LLVMValueRef instruction)
    {
        if (instruction.IsConditional)
        {
            var branchOpcode = EmitBranchCondition(instruction.Condition);

            var trueBlock = instruction.GetSuccessor(0);

            Label? trueBlockPhiLabel = null;
            if (trueBlock.ContainsPhiNodes())
            {
                trueBlockPhiLabel = ILGenerator.DefineLabel();
                ILGenerator.Emit(branchOpcode, trueBlockPhiLabel.Value);
            }
            else
            {
                ILGenerator.Emit(branchOpcode, GetOrCreateLabel(trueBlock));
            }

            EmitBranchUnconditional(instruction, instruction.GetSuccessor(1));

            if (trueBlockPhiLabel != null)
            {
                ILGenerator.MarkLabel(trueBlockPhiLabel.Value);
                EmitPhiValues(instruction.InstructionParent, trueBlock);
                ILGenerator.Emit(OpCodes.Br, GetOrCreateLabel(trueBlock));
            }
        }
        else
        {
            EmitBranchUnconditional(instruction, instruction.GetSuccessor(0));
        }
    }

    private OpCode EmitBranchCondition(LLVMValueRef condition)
    {
        if (CanPushToStack(condition)
            && condition.InstructionOpcode == LLVMOpcode.LLVMICmp
            && condition.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            EmitValue(condition.GetOperand(0));
            EmitValue(condition.GetOperand(1));

            return condition.ICmpPredicate switch
            {
                LLVMIntPredicate.LLVMIntEQ => OpCodes.Beq,
                LLVMIntPredicate.LLVMIntNE => OpCodes.Bne_Un,
                LLVMIntPredicate.LLVMIntSGE => OpCodes.Bge,
                LLVMIntPredicate.LLVMIntSGT => OpCodes.Bgt,
                LLVMIntPredicate.LLVMIntSLT => OpCodes.Blt,
                LLVMIntPredicate.LLVMIntSLE => OpCodes.Ble,
                LLVMIntPredicate.LLVMIntUGT => OpCodes.Bgt_Un,
                LLVMIntPredicate.LLVMIntULE => OpCodes.Ble_Un,
                LLVMIntPredicate.LLVMIntULT => OpCodes.Blt_Un,
                _ => throw new NotImplementedException($"Branch condition integer comparison {condition.ICmpPredicate} not implemented: {condition}"),
            };
        }
        else if (CanPushToStack(condition)
            && condition.InstructionOpcode == LLVMOpcode.LLVMFCmp
            && condition.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            EmitValue(condition.GetOperand(0));
            EmitValue(condition.GetOperand(1));

            return condition.FCmpPredicate switch
            {
                LLVMRealPredicate.LLVMRealOEQ => OpCodes.Beq,
                LLVMRealPredicate.LLVMRealOGE => OpCodes.Bge,
                LLVMRealPredicate.LLVMRealOGT => OpCodes.Bgt,
                LLVMRealPredicate.LLVMRealOLE => OpCodes.Ble,
                LLVMRealPredicate.LLVMRealOLT => OpCodes.Blt,
                LLVMRealPredicate.LLVMRealUGE => OpCodes.Bge_Un,
                LLVMRealPredicate.LLVMRealULE => OpCodes.Ble_Un,
                LLVMRealPredicate.LLVMRealULT => OpCodes.Blt_Un,
                LLVMRealPredicate.LLVMRealUNE => OpCodes.Bne_Un,
                _ => throw new NotImplementedException($"Branch condition float comparison {condition.FCmpPredicate} not implemented: {condition}"),
            };
        }
        else
        {
            EmitValue(condition);

            return OpCodes.Brtrue;
        }
    }

    private void EmitBranchUnconditional(LLVMValueRef brInstruction, LLVMBasicBlockRef to)
    {
        if (to.ContainsPhiNodes())
        {
            EmitPhiValues(brInstruction.InstructionParent, to);
        }
        ILGenerator.Emit(OpCodes.Br, GetOrCreateLabel(to));
    }

    private unsafe void EmitCall(LLVMValueRef instruction)
    {
        var operands = instruction.GetOperands().ToArray();

        var functionToCall = operands[^1];

        if (instruction.IsAIntrinsicInst != null)
        {
            var functionName = functionToCall.Name;
            if (IntrinsicFunctions.LLVMIntrinsics.TryGetValue(functionName, out var intrinsic))
            {
                var callContext = new IntrinsicFunctionCallContext(
                    _method,
                    Locals,
                    ILGenerator,
                    operands,
                    EmitValue);
                intrinsic.BuildCall(callContext);
                return;
            }
            else
            {
                throw new NotImplementedException($"Unknown LLVM intrinsic: {functionName}");
            }
        }


        for (var i = 0; i < operands.Length - 1; i++)
        {
            EmitValue(operands[i]);
        }

        var functionType = (LLVMTypeRef)LLVM.GetCalledFunctionType(instruction);

        var varArgsParameterTypes = Array.Empty<Type>();
        var isVarArg = functionType.IsFunctionVarArg;
        if (isVarArg)
        {
            var parameters = functionType.ParamTypes;
            varArgsParameterTypes = new Type[operands.Length - 1 - parameters.Length];
            for (var i = 0; i < varArgsParameterTypes.Length; i++)
            {
                varArgsParameterTypes[i] = TypeSystem.GetMsilType(operands[i + parameters.Length].TypeOf);
            }
        }

        var numParameters = (int)functionType.ParamTypesCount;

        if (functionToCall.Kind != LLVMValueKind.LLVMFunctionValueKind)
        {
            // This is a function pointer invocation.

            EmitValue(functionToCall);

            var returnType = TypeSystem.GetMsilType(instruction.TypeOf);

            var parameterTypes = new Type[numParameters];
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                parameterTypes[i] = TypeSystem.GetMsilType(operands[i].TypeOf);
            }

            if (isVarArg)
            {
                ILGenerator.EmitCalli(
                    OpCodes.Calli,
                    CallingConventions.VarArgs,
                    returnType,
                    parameterTypes,
                    varArgsParameterTypes);
            }
            else
            {
                ILGenerator.EmitCalli(
                    OpCodes.Calli,
                    System.Runtime.InteropServices.CallingConvention.Cdecl,
                    returnType,
                    parameterTypes);
            }

            return;
        }

        var method = CompiledModule.GetFunction(functionToCall);

        ILGenerator.EmitCall(
            OpCodes.Call,
            method,
            varArgsParameterTypes);
    }

    private unsafe void HandleDebugDeclare(LLVMValueRef instruction)
    {
        var value = instruction.GetOperand(0).MDNodeOperands[0];

        var diLocalVariable = instruction.GetOperand(1);
        var diLocalVariableName = diLocalVariable.GetDILocalVariableName();

        var diLocalVariableArg = diLocalVariable.GetDILocalVariableArg();
        if (diLocalVariableArg != null)
        {
            // Parameter information. We'll have already used this to set the parameter name.
        }
        else
        {
            if (Locals.TryGetValue(value, out var local))
            {
                // Local information.
                local.SetLocalSymInfo(diLocalVariableName);
            }
            //else
            //{
            //    // Global information
            //    var global = context.CompilationContext.Globals[value];
            //}
        }

        //var expression = instruction.GetOperand(2).AsMetadata();

        //var dbgMetadata = instruction.GetMetadata("dbg");

        //var line = dbgMetadata.GetDILocationLine();
        //var scope = dbgMetadata.GetDILocationScope();

        //var file = scope.GetDIScopeFile();

        //var symbolDocumentWriter = TypeSystem.GetDocument(file);
    }

    private unsafe void EmitGetElementPtr(LLVMValueRef instruction)
    {
        // TODO: If every operand is const, call GetElementPtrConst

        var pointer = instruction.GetOperand(0);
        EmitValue(pointer);

        var sourceElementType = (LLVMTypeRef)LLVM.GetGEPSourceElementType(instruction);
        var currentType = sourceElementType;

        // First index operand always indexes into the source element pointer type.
        EmitIndexedPtr(instruction.GetOperand(1), currentType);

        for (var i = 2u; i < instruction.OperandCount; i++)
        {
            var index = instruction.GetOperand(i);

            switch (currentType.Kind)
            {
                case LLVMTypeKind.LLVMArrayTypeKind:
                    EmitIndexedPtr(index, currentType.ElementType);
                    currentType = currentType.ElementType;
                    break;

                case LLVMTypeKind.LLVMIntegerTypeKind:
                    EmitIndexedPtr(index, currentType);
                    break;

                case LLVMTypeKind.LLVMStructTypeKind:
                    var structType = TypeSystem.GetMsilType(currentType);
                    uint fieldIndex;
                    if (index.Kind == LLVMValueKind.LLVMConstantIntValueKind)
                    {
                        fieldIndex = (uint)index.ConstIntSExt;
                    }
                    else
                    {
                        throw new NotImplementedException($"GetElementPtr not implemented for index {index} for type {currentType}: {instruction}");
                    }
                    var field = structType.GetFields()[fieldIndex];
                    ILGenerator.Emit(OpCodes.Ldflda, field);
                    ILGenerator.Emit(OpCodes.Conv_U);
                    currentType = currentType.StructGetTypeAtIndex(fieldIndex);
                    break;

                case LLVMTypeKind.LLVMVectorTypeKind:
                    EmitIndexedPtr(index, currentType.ElementType);
                    currentType = currentType.ElementType;
                    break;

                default:
                    throw new NotImplementedException($"GetElementPtr not implemented for index {index} for type {currentType}: {instruction}");
            }
        }
    }

    private void EmitIndexedPtr(LLVMValueRef index, LLVMTypeRef currentType)
    {
        if (index.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind)
        {
            // TODO: Implement vector indices.
            throw new NotImplementedException();
        }

        var sizeInBytes = (long)TypeSystem.GetSizeOfTypeInBytes(currentType);

        if (index.Kind == LLVMValueKind.LLVMConstantIntValueKind)
        {
            if (index.ConstIntSExt == 0)
            {
                // No need to do anything.
            }
            else
            {
                //if (index.ConstIntSExt > 0)
                {
                    ILGenerator.Emit(OpCodes.Ldc_I8, sizeInBytes * index.ConstIntSExt);
                    ILGenerator.Emit(OpCodes.Conv_U);
                    ILGenerator.Emit(OpCodes.Add);
                }
                //else
                //{
                //    throw new NotImplementedException($"Index not implemented: {index}");
                //}
            }
        }
        else
        {
            EmitValue(index);

            if (index.TypeOf.IntWidth != 64)
            {
                throw new NotImplementedException();
            }

            if (sizeInBytes != 1)
            {
                ILGenerator.Emit(OpCodes.Ldc_I8, sizeInBytes);
                ILGenerator.Emit(OpCodes.Mul);
            }

            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);
        }
    }

    private void EmitPhiValues(LLVMBasicBlockRef from, LLVMBasicBlockRef to)
    {
        // Phi values might refer to each other recursively, e.g.
        // %2 = phi i32 [ 1, %0 ], [ %3, %1 ]
        // %3 = phi i32 [ 0, %0 ], [ %2, %1 ]
        //
        // ... so we take advantage of the stack-based nature of IL
        // to push all the phi values to the stack, then pop them
        // into phi locals.

        var phiStack = new Stack<LLVMValueRef>();

        foreach (var phiInstruction in to.GetInstructions()
            .Where(x => x.InstructionOpcode == LLVMOpcode.LLVMPHI))
        {
            var incomingValue = phiInstruction.GetIncomingValueForBlock(from);
            EmitValue(incomingValue);

            phiStack.Push(phiInstruction);
        }

        while (phiStack.Count > 0)
        {
            var phiInstruction = phiStack.Pop();
            ILGenerator.Emit(OpCodes.Stloc, PhiLocals[phiInstruction]);
        }
    }

    private readonly record struct PhiInstructionAndIncomingValue(LLVMValueRef PhiInstruction, LLVMValueRef IncomingValue);

    private void EmitSelect(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);
        switch (operand0.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:
                var trueLabel = ILGenerator.DefineLabel();
                var endLabel = ILGenerator.DefineLabel();

                var branchOpcode = EmitBranchCondition(operand0);
                ILGenerator.Emit(branchOpcode, trueLabel);

                EmitValue(instruction.GetOperand(2));
                ILGenerator.Emit(OpCodes.Br, endLabel);

                ILGenerator.MarkLabel(trueLabel);
                EmitValue(instruction.GetOperand(1));

                ILGenerator.MarkLabel(endLabel);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                var operand1 = instruction.GetOperand(1);

                EmitValue(operand0);

                // Now we need to convert condition from <2 x i1> to same type as left and right operands.

                // Store to local because we'll need to reference it multiple times below.
                var intermediateLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilVectorType(operand0.TypeOf));
                ILGenerator.Emit(OpCodes.Stloc, intermediateLocal);

                // We need to extend the result from e.g. Vector16<sbyte> to Vector64<int>,
                // something like this:
                // Vector64.Create((int)input[0], (int)input[1]);

                var inputVectorType = TypeSystem.GetMsilVectorType(operand0.TypeOf);
                var operand1ElementSizeInBits = TypeSystem.GetSizeOfTypeInBits(operand1.TypeOf.ElementType);
                for (var i = 0; i < instruction.TypeOf.VectorSize; i++)
                {
                    ILGenerator.Emit(OpCodes.Ldloca, intermediateLocal);
                    ILGenerator.Emit(OpCodes.Ldc_I4, i);
                    ILGenerator.Emit(OpCodes.Call, inputVectorType.GetMethodStrict("get_Item"));

                    // If integer bit width is larger than 32, we extend.
                    if (operand1ElementSizeInBits > 32)
                    {
                        ILGenerator.Emit(OpCodes.Conv_I8);
                    }
                }

                var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(operand1.TypeOf);
                var createMethodArgumentTypes = new Type[operand1.TypeOf.VectorSize];
                // Integer type with same bitwidth as operand1 element type.
                Array.Fill(createMethodArgumentTypes, TypeSystem.GetIntegerType(TypeSystem.RoundUpToTypeSize(operand1ElementSizeInBits)));
                var createMethod = nonGenericVectorType.GetMethodStrict(nameof(Vector128.Create), createMethodArgumentTypes);
                ILGenerator.Emit(OpCodes.Call, createMethod);

                // If necessary, call cast method, e.g. AsSingle.
                switch (operand1.TypeOf.ElementType.Kind)
                {
                    case LLVMTypeKind.LLVMDoubleTypeKind:
                        ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsDouble)).MakeGenericMethod(createMethodArgumentTypes[0]));
                        break;

                    case LLVMTypeKind.LLVMFloatTypeKind:
                        ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsSingle)).MakeGenericMethod(createMethodArgumentTypes[0]));
                        break;

                    case LLVMTypeKind.LLVMIntegerTypeKind:
                        // Nothing to do.
                        break;

                    default:
                        throw new NotImplementedException($"Unsupported operand element type: {operand1.TypeOf.ElementType}");
                }

                EmitValue(operand1);
                EmitValue(instruction.GetOperand(2));

                var elementType = TypeSystem.GetMsilVectorElementType(operand1.TypeOf.ElementType);
                var conditionalSelectMethod = nonGenericVectorType
                    .GetStaticMethodStrict(nameof(Vector128.ConditionalSelect))
                    .MakeGenericMethod(elementType);
                ILGenerator.Emit(OpCodes.Call, conditionalSelectMethod);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private readonly record struct SwitchCase(int ConstantValue, LLVMValueRef Value, Label DestinationLabel)
        : IComparable<SwitchCase>
    {
        public int CompareTo(SwitchCase other)
        {
            return ConstantValue.CompareTo(other.ConstantValue);
        }
    }

    private void EmitSwitch(LLVMValueRef instruction)
    {
        // TODO: We could do a better job here at filling in the gaps if we have good enough density,
        // similar to what Roslyn does with jump tables:
        // https://github.com/dotnet/roslyn/blob/36e1fe3c27adb70b3ad49c9d51d7cc19d88e656e/src/Compilers/Core/Portable/CodeGen/SwitchIntegralJumpTableEmitter.cs#L67C23-L67C36

        var operands = instruction.GetOperands().ToList();

        // Operand 0 is the condition value.
        var condition = operands[0];

        if (condition.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind
            || condition.TypeOf.IntWidth > 32)
        {
            throw new NotImplementedException($"Unsupported switch condition: {condition}");
        }

        var phiBlocksToEmit = new List<(Label, LLVMBasicBlockRef)>();

        // Operand 1 is the default destination.

        // Operand 2+ are the cases in the format:
        // - {n+0} = case value
        // - {n+1} = case destination
        var cases = new List<SwitchCase>();
        for (var i = 2; i < operands.Count; i += 2)
        {
            var caseValueType = operands[i].Kind;

            if (operands[i].Kind != LLVMValueKind.LLVMConstantIntValueKind
                || operands[i].TypeOf.IntWidth > 32)
            {
                throw new NotImplementedException();
            }

            var caseValue = (int)operands[i].ConstIntSExt;

            var destinationBlock = operands[i + 1].AsBasicBlock();
            Label destinationLabel;
            if (destinationBlock.ContainsPhiNodes())
            {
                destinationLabel = ILGenerator.DefineLabel();
                phiBlocksToEmit.Add((destinationLabel, destinationBlock));
            }
            else
            {
                destinationLabel = GetOrCreateLabel(destinationBlock);
            }

            cases.Add(new SwitchCase(caseValue, operands[i], destinationLabel));
        }
        cases.Sort();

        while (cases.Count > 0)
        {
            var caseValue = cases[0].ConstantValue;

            // Include as many sequential values as we can.
            var endIndex = 1;
            while (endIndex < cases.Count && cases[endIndex].ConstantValue == caseValue + endIndex)
            {
                endIndex++;
            }

            EmitValue(condition);
            if (caseValue != 0)
            {
                EmitValue(cases[0].Value);
            }

            // If we have 2 or more cases, make a switch instruction.
            if (endIndex > 1)
            {
                if (caseValue != 0)
                {
                    ILGenerator.Emit(OpCodes.Sub);
                }
                var jumpTable = cases
                    .Take(endIndex)
                    .Select(x => x.DestinationLabel)
                    .ToArray();
                ILGenerator.Emit(OpCodes.Switch, jumpTable);
                cases.RemoveRange(0, endIndex);
            }
            else // Otherwise, do a normal conditional branch.
            {
                if (caseValue == 0)
                {
                    ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                }
                ILGenerator.Emit(OpCodes.Beq, cases[0].DestinationLabel);
                cases.RemoveAt(0);
            }
        }

        ILGenerator.Emit(OpCodes.Br, GetOrCreateLabel(instruction.SwitchDefaultDest));

        foreach (var (phiLabel, destinationBlock) in phiBlocksToEmit)
        {
            ILGenerator.MarkLabel(phiLabel);
            EmitPhiValues(instruction.InstructionParent, destinationBlock);
            ILGenerator.Emit(OpCodes.Br, GetOrCreateLabel(destinationBlock));
        }
    }

    private void EmitValue(LLVMValueRef valueRef)
    {
        if (valueRef.IsConstant)
        {
            EmitConstantValue(valueRef, valueRef.TypeOf);
        }
        else if (Locals.TryGetValue(valueRef, out var local))
        {
            EmitLoadLocal(valueRef, local);
        }
        else if (PhiLocals.TryGetValue(valueRef, out var phiLocal))
        {
            ILGenerator.Emit(OpCodes.Ldloc, phiLocal);
        }
        else if (Parameters.TryGetValue(valueRef, out var parameter))
        {
            ILGenerator.Emit(OpCodes.Ldarg, parameter.Position - 1);
        }
        else if (CanPushToStack(valueRef))
        {
            CompileInstructionValue(valueRef);
        }
        else if (valueRef.IsAInstruction != null)
        {
            // We get here if an assignment is used before it's assigned.
            var newLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(valueRef.TypeOf));
            Locals.Add(valueRef, newLocal);

            EmitLoadLocal(valueRef, newLocal);
        }
        else
        {
            throw new InvalidOperationException($"Unexpected value: {valueRef}");
        }
    }

    private void EmitLoadLocal(LLVMValueRef valueRef, LocalBuilder local)
    {
        if (valueRef.IsAAllocaInst != null && valueRef.AllocaHasConstantNumElements())
        {
            ILGenerator.Emit(OpCodes.Ldloca, local);
        }
        else
        {
            ILGenerator.Emit(OpCodes.Ldloc, local);
        }
    }

    private void EmitLoad(LLVMValueRef instruction)
    {
        var valueRef = instruction.GetOperand(0);

        EmitValue(valueRef);
        EmitLoadIndirect(instruction.TypeOf);

        // TODO: Atomic load?
    }

    private void EmitLoadIndirect(LLVMTypeRef typeRef)
    {
        switch (typeRef.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Ldind_R8);
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Ldind_R4);
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                switch (typeRef.IntWidth)
                {
                    case 1:
                    case 8:
                        ILGenerator.Emit(OpCodes.Ldind_I1);
                        break;

                    case 16:
                        ILGenerator.Emit(OpCodes.Ldind_I2);
                        break;

                    case 32:
                        ILGenerator.Emit(OpCodes.Ldind_I4);
                        break;

                    case 64:
                        ILGenerator.Emit(OpCodes.Ldind_I8);
                        break;

                    default:
                        throw new NotImplementedException($"Int width {typeRef.IntWidth} not implemented: {typeRef}");
                }
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                ILGenerator.Emit(OpCodes.Ldind_I);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                ILGenerator.Emit(OpCodes.Ldobj, TypeSystem.GetMsilType(typeRef));
                break;

            default:
                throw new NotImplementedException($"Unsupported type {typeRef.Kind}");
        }
    }

    private void EmitStoreResult(LLVMValueRef instruction)
    {
        // TODO: Declare locals upfront.
        if (!Locals.TryGetValue(instruction, out var local))
        {
            local = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(instruction.TypeOf));
            Locals.Add(instruction, local);
        }
        EmitStloc(instruction, instruction.TypeOf);
    }

    private void EmitStloc(LLVMValueRef valueRef, LLVMTypeRef type)
    {
        if (Locals.TryGetValue(valueRef, out var local))
        {
            ILGenerator.Emit(OpCodes.Stloc, local);
        }
        else
        {
            EmitValue(valueRef);
            EmitStoreIndirect(type);
        }
    }
}