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

internal sealed partial class FunctionILEmitter : ILEmitter
{
    private readonly MethodInfo _method;
    private readonly LLVMValueRef _function;

    private readonly Dictionary<LLVMValueRef, bool> CanPushToStackLookup = [];
    private readonly Dictionary<LLVMValueRef, MemoryEffect> TreeMemoryEffects = [];

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
        foreach (var basicBlock in _function.GetBasicBlocks())
        {
            var blockInstructions = basicBlock.GetInstructions().ToList();
            for (var i = 0; i < blockInstructions.Count; i++)
            {
                CanPushToStackLookup.Add(blockInstructions[i], CanPushToStack(blockInstructions, i));
            }
        }

        var functionParams = _function.GetParams();
        for (var i = 0; i < functionParams.Length; i++)
        {
            var parameter = functionParams[i];
            var parameterIndex = i + 1;

            var parameterName = _function.GetParameterName(parameterIndex);

            var parameterBuilder = compiledFunction.MethodBuilder.DefineParameter(
                parameterIndex,
                ParameterAttributes.None, // TODO
                parameterName);

            Parameters.Add(parameter, parameterBuilder);
        }
    }

    private bool CanPushToStack(List<LLVMValueRef> instructions, int index)
    {
        var instruction = instructions[index];
        var users = instruction.GetUses().ToList();

        // An instruction pushed to the stack is evaluated where its user is, together with every operand
        // that was itself pushed to the stack. Record what that whole expression does to memory.
        var effect = instruction.GetMemoryEffect();
        foreach (var operand in instruction.GetOperands())
        {
            if (TreeMemoryEffects.TryGetValue(operand, out var operandEffect) && CanPushToStack(operand))
            {
                effect = (MemoryEffect)Math.Max((int)effect, (int)operandEffect);
            }
        }
        TreeMemoryEffects.Add(instruction, effect);

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

        switch (effect)
        {
            case MemoryEffect.None:
                return true;

            case MemoryEffect.Reads:
                // Make sure the expression is evaluated before anything that might change what it reads.
                for (var j = index + 1; j < instructions.Count; j++)
                {
                    if (instructions[j] == user)
                    {
                        return true;
                    }
                    if (instructions[j].GetMemoryEffect() == MemoryEffect.Writes)
                    {
                        return false;
                    }
                }
                throw new InvalidOperationException("Shouldn't be here");

            default:
                return false;
        }
    }

    public void Compile()
    {
        foreach (var basicBlock in _function.GetBasicBlocks())
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

        foreach (var basicBlock in _function.GetBasicBlocks())
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
                EmitBinaryOperation(instruction, OpCodes.Add, nameof(Vector128.Add), Signedness.Any);
                break;

            case LLVMOpcode.LLVMAnd:
                EmitBinaryOperation(instruction, OpCodes.And, nameof(Vector128.BitwiseAnd), Signedness.Any);
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

            case LLVMOpcode.LLVMExtractValue:
                EmitExtractValue(instruction);
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

            case LLVMOpcode.LLVMInsertValue:
                EmitInsertValue(instruction);
                break;

            case LLVMOpcode.LLVMLoad:
                EmitLoad(instruction);
                break;

            case LLVMOpcode.LLVMFDiv:
                EmitBinaryOperation(instruction, OpCodes.Div, nameof(Vector128.Divide), Signedness.Signed);
                break;

            case LLVMOpcode.LLVMFMul:
                EmitBinaryOperation(instruction, OpCodes.Mul, nameof(Vector128.Multiply), Signedness.Any);
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

            case LLVMOpcode.LLVMFRem:
                EmitBinaryOperation(instruction, OpCodes.Rem, "SignedRemainder", Signedness.Signed);
                break;

            case LLVMOpcode.LLVMIntToPtr:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMLShr:
                EmitBinaryOperation(instruction, OpCodes.Shr_Un, nameof(Vector128.ShiftRightLogical), Signedness.Unsigned);
                break;

            case LLVMOpcode.LLVMMul:
                EmitBinaryOperation(instruction, OpCodes.Mul, nameof(Vector128.Multiply), Signedness.Any);
                break;

            case LLVMOpcode.LLVMOr:
                EmitBinaryOperation(instruction, OpCodes.Or, nameof(Vector128.BitwiseOr), Signedness.Any);
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
                EmitBinaryOperation(instruction, OpCodes.Shl, nameof(Vector128.ShiftLeft), Signedness.Any);
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
                EmitBinaryOperation(instruction, OpCodes.Sub, nameof(Vector128.Subtract), Signedness.Any);
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
                EmitBinaryOperation(instruction, OpCodes.Xor, nameof(Vector128.Xor), Signedness.Any);
                break;

            case LLVMOpcode.LLVMZExt:
                EmitConversion(instruction, Signedness.Unsigned);
                break;

            default:
                throw new NotImplementedException($"Instruction {instruction.InstructionOpcode} is not implemented: {instruction}");
        }
    }

    /// <summary>
    /// <c>atomicrmw</c> on an 8, 16, 32 or 64-bit integer, or a <c>float</c> or <c>double</c>. Uses the <see cref="Interlocked"/>
    /// method when there is one (<c>Exchange</c> and, on integers, <c>And</c> and <c>Or</c> on every width and <c>Add</c> on
    /// 32 and 64 bits); everything else is a compare-exchange loop in <see cref="AtomicHelper"/>. The result is the old value.
    /// </summary>
    private void EmitAtomicRMW(LLVMValueRef instruction)
    {
        var pointer = instruction.GetOperand(0);
        var value = instruction.GetOperand(1);
        var operation = instruction.AtomicRMWBinOp;

        if (value.TypeOf.Kind is LLVMTypeKind.LLVMFloatTypeKind or LLVMTypeKind.LLVMDoubleTypeKind)
        {
            EmitFloatAtomicRMW(instruction);
            return;
        }

        if (value.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind || value.TypeOf.IntWidth is not (8 or 16 or 32 or 64))
        {
            throw new NotImplementedException($"Atomic RMW on type {value.TypeOf} not implemented: {instruction}");
        }

        // LLVM's umax/umin are max/min on the unsigned type; the other operations don't care.
        var unsigned = operation is LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpUMax or LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpUMin;
        var bits = (int)value.TypeOf.IntWidth;
        var integerType = bits switch
        {
            8 => unsigned ? typeof(byte) : typeof(sbyte),
            16 => unsigned ? typeof(ushort) : typeof(short),
            32 => unsigned ? typeof(uint) : typeof(int),
            _ => unsigned ? typeof(ulong) : typeof(long),
        };

        EmitValue(pointer);
        EmitValue(value);

        // The methods take the operand as its exact type, so drop what the stack holds above the logical width.
        if (bits < 32)
        {
            ILGenerator.Emit(integerType switch
            {
                _ when integerType == typeof(sbyte) => OpCodes.Conv_I1,
                _ when integerType == typeof(byte) => OpCodes.Conv_U1,
                _ when integerType == typeof(short) => OpCodes.Conv_I2,
                _ => OpCodes.Conv_U2,
            });
        }

        var byRef = integerType.MakeByRefType();
        switch (operation)
        {
            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg:
                ILGenerator.Emit(OpCodes.Call, typeof(Interlocked).GetMethodStrict(nameof(Interlocked.Exchange), [byRef, integerType]));
                return;

            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd:
            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr:
                var name = operation == LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd ? nameof(Interlocked.And) : nameof(Interlocked.Or);
                ILGenerator.Emit(OpCodes.Call, bits >= 32
                    ? typeof(Interlocked).GetMethodStrict(name, [byRef, integerType])
                    : typeof(Interlocked).GetGenericMethodDefinitionStrict(name).MakeGenericMethod(integerType));
                return;

            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd when bits >= 32:
            case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub when bits >= 32:
                // Interlocked.Add returns the new value; the old one is that minus the operand. Sub adds the negation.
                var operand = ILGenerator.DeclareLocal(integerType);
                var subtract = operation == LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub;
                ILGenerator.Emit(OpCodes.Stloc, operand);
                ILGenerator.Emit(OpCodes.Ldloc, operand);
                if (subtract)
                {
                    ILGenerator.Emit(OpCodes.Neg);
                }

                ILGenerator.Emit(OpCodes.Call, typeof(Interlocked).GetMethodStrict(nameof(Interlocked.Add), [byRef, integerType]));
                ILGenerator.Emit(OpCodes.Ldloc, operand);
                ILGenerator.Emit(subtract ? OpCodes.Add : OpCodes.Sub);
                return;
        }

        var operationDefinition = operation switch
        {
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd => typeof(AtomicHelper.Add<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub => typeof(AtomicHelper.Subtract<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpNand => typeof(AtomicHelper.Nand<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXor => typeof(AtomicHelper.Xor<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpMax or LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpUMax => typeof(AtomicHelper.Max<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpMin or LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpUMin => typeof(AtomicHelper.Min<>),
            _ => throw new NotImplementedException($"Atomic RMW operation {operation} not implemented: {instruction}"),
        };

        ILGenerator.Emit(
            OpCodes.Call,
            typeof(AtomicHelper)
                .GetMethodStrict(nameof(AtomicHelper.ReadModifyWrite))
                .MakeGenericMethod(integerType, operationDefinition.MakeGenericType(integerType)));
    }

    private void EmitFloatAtomicRMW(LLVMValueRef instruction)
    {
        var operation = instruction.AtomicRMWBinOp;
        var floatType = instruction.GetOperand(1).TypeOf.Kind == LLVMTypeKind.LLVMFloatTypeKind ? typeof(float) : typeof(double);

        EmitValue(instruction.GetOperand(0));
        EmitValue(instruction.GetOperand(1));

        if (operation == LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg)
        {
            ILGenerator.Emit(OpCodes.Call, typeof(Interlocked).GetMethodStrict(nameof(Interlocked.Exchange), [floatType.MakeByRefType(), floatType]));
            return;
        }

        var operationDefinition = operation switch
        {
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpFAdd => typeof(AtomicHelper.FloatAdd<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpFSub => typeof(AtomicHelper.FloatSubtract<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpFMax => typeof(AtomicHelper.FloatMax<>),
            LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpFMin => typeof(AtomicHelper.FloatMin<>),
            _ => throw new NotImplementedException($"Atomic RMW operation {operation} not implemented: {instruction}"),
        };

        ILGenerator.Emit(
            OpCodes.Call,
            typeof(AtomicHelper)
                .GetMethodStrict(nameof(AtomicHelper.ReadModifyWriteFloat))
                .MakeGenericMethod(floatType, operationDefinition.MakeGenericType(floatType)));
    }

    private void EmitBitCast(LLVMValueRef instruction)
    {
        var fromType = instruction.GetOperand(0).TypeOf;
        var toType = instruction.TypeOf;

        if (fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && IsMaskType(toType))
        {
            // Lane i is all ones when bit i is set. Only the low N bits of the integer are defined.
            if (fromType.IntWidth > 64)
            {
                throw new NotImplementedException($"Unsupported bitcast from type {fromType} to type {toType}: {instruction}");
            }

            EmitIntegerOperand(instruction.GetOperand(0), Signedness.Unsigned);
            ILGenerator.Emit(OpCodes.Conv_U8);
            ILGenerator.Emit(
                OpCodes.Call,
                typeof(VectorUtility).GetStaticMethodStrict($"MaskFromBits{toType.VectorSize}"));
            return;
        }

        EmitValue(instruction.GetOperand(0));

        var fromTypeMsil = TypeSystem.GetMsilType(fromType);
        var toTypeMsil = TypeSystem.GetMsilType(toType);

        if (!IsMaskType(fromType) && (TypeSystem.IsPaddedVector(fromType) || TypeSystem.IsPaddedVector(toType)))
        {
            // A padded vector (<2 x i8> in a Vector64<sbyte>) differs in size from the type it is cast to or
            // from, so Unsafe.BitCast doesn't apply. Go through memory, where a vector has exactly its own bytes:
            // the padding lanes are left behind, or the missing ones are zero.
            var castLocal = ILGenerator.DeclareLocal(fromTypeMsil);
            ILGenerator.Emit(OpCodes.Stloc, castLocal);
            ILGenerator.Emit(OpCodes.Ldloca, castLocal);
            ILGenerator.Emit(OpCodes.Conv_U);
            EmitLoadIndirect(toType);
            return;
        }

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
        EmitIntegerOperand(instruction.GetOperand(1), Signedness.Unsigned);
        ILGenerator.Emit(OpCodes.Conv_I4);

        var vectorType = instruction.GetOperand(0).TypeOf;
        var getElementMethod = TypeSystem.GetNonGenericVectorType(vectorType)
            .GetStaticMethodStrict(nameof(Vector128.GetElement))
            .MakeGenericMethod(TypeSystem.GetMsilVectorElementType(vectorType.ElementType));
        ILGenerator.Emit(OpCodes.Call, getElementMethod);
    }

    private void EmitExtractValue(LLVMValueRef instruction)
    {
        var aggregateOperand = instruction.GetOperand(0);

        EmitValueAddress(aggregateOperand);
        var elementType = EmitAggregateElementAddress(aggregateOperand.TypeOf, instruction.GetIndices());
        EmitLoadIndirect(elementType);
    }

    /// <summary>
    /// With the address of an aggregate on the stack, replaces it with the address of the member that the
    /// index path leads to (an array element or a struct field, at any depth), and returns that member's type.
    /// </summary>
    private LLVMTypeRef EmitAggregateElementAddress(LLVMTypeRef aggregateType, ReadOnlySpan<uint> indices)
    {
        var currentType = aggregateType;

        foreach (var index in indices)
        {
            switch (currentType.Kind)
            {
                case LLVMTypeKind.LLVMArrayTypeKind:
                    // Get a ref to the element via the InlineArrayElementRef helper.
                    var bufferType = TypeSystem.GetMsilType(currentType);
                    var elementMemoryType = TypeSystem.GetMsilMemoryType(currentType.ElementType);
                    ILGenerator.Emit(OpCodes.Ldc_I4, (int)index);
                    ILGenerator.Emit(OpCodes.Call, CompiledModule.GetOrCreateInlineArrayElementRef().MakeGenericMethod(bufferType, elementMemoryType));
                    currentType = currentType.ElementType;
                    break;

                case LLVMTypeKind.LLVMStructTypeKind:
                    ILGenerator.Emit(OpCodes.Ldflda, TypeSystem.GetMsilType(currentType).GetFields()[index]);
                    currentType = currentType.StructGetTypeAtIndex(index);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported aggregate type for extractvalue/insertvalue: {currentType}");
            }
        }

        return currentType;
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
                var elementSizeInBytes = TypeSystem.GetSizeOfTypeInBytes(instruction.GetAllocatedType());
                EmitValue(numElements);
                if (elementSizeInBytes != 1)
                {
                    ILGenerator.Emit(OpCodes.Ldc_I8, (long)elementSizeInBytes);
                    ILGenerator.Emit(OpCodes.Mul);
                }
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
            var scalarValueType = TypeSystem.GetMsilVectorElementType(scalarValue.TypeOf);
            if (scalarValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
            {
                ILGenerator.Emit(OpCodes.Conv_I);
            }

            // Create vector from scalar value.
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
            && maskIndices.SequenceEqual(Enumerable.Range(0, maskIndices.Length))
            && !TypeSystem.IsPaddedVector(sourceVector0.TypeOf))
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

        // The lanes are laid out as in the .NET vector: an i1 lane is a byte (not the zero bytes
        // GetSizeOfTypeInBytes gives for one bit) and an i24 lane an int.
        var elementSizeInBytes = instruction.TypeOf.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind
            ? TypeSystem.RoundUpToTypeSize((int)instruction.TypeOf.ElementType.IntWidth) / 8
            : TypeSystem.GetSizeOfTypeInBytes(instruction.TypeOf.ElementType);
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
        EmitIntegerOperand(instruction.GetOperand(2), Signedness.Unsigned);
        ILGenerator.Emit(OpCodes.Conv_I4);

        // Value
        var valueOperand = instruction.GetOperand(1);
        EmitValue(valueOperand);
        var valueType = TypeSystem.GetMsilVectorElementType(valueOperand.TypeOf);
        if (valueOperand.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            ILGenerator.Emit(OpCodes.Conv_I);
        }

        EmitVectorWithElement(
            TypeSystem.GetNonGenericVectorType(vectorOperand.TypeOf),
            valueType);
    }

    private void EmitVectorWithElement(Type nonGenericVectorType, Type valueType)
    {
        var withElementMethod = nonGenericVectorType
            .GetStaticMethodStrict(nameof(Vector128.WithElement))
            .MakeGenericMethod(valueType);
        ILGenerator.Emit(OpCodes.Call, withElementMethod);
    }

    private void EmitInsertValue(LLVMValueRef instruction)
    {
        var aggregateOperand = instruction.GetOperand(0);
        var aggregateType = aggregateOperand.TypeOf;

        // Store the aggregate to a local so we can take its address.
        var aggregateLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(aggregateType));
        EmitValue(aggregateOperand);
        ILGenerator.Emit(OpCodes.Stloc, aggregateLocal);

        // Store the value through the address of the member, then leave the modified aggregate as the result.
        ILGenerator.Emit(OpCodes.Ldloca, aggregateLocal);
        var elementType = EmitAggregateElementAddress(aggregateType, instruction.GetIndices());
        EmitValue(instruction.GetOperand(1));
        EmitStoreIndirect(elementType);
        ILGenerator.Emit(OpCodes.Ldloc, aggregateLocal);
    }

    /// <summary>
    /// Pushes an integer operand, extended from its logical width the way its consumer reads it. Operands
    /// that aren't integers, or are 32, 64 or 128 bits wide, are pushed as they are.
    /// </summary>
    private void EmitIntegerOperand(LLVMValueRef operand, Signedness signedness)
    {
        EmitValue(operand);

        if (operand.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            EmitNormalizedInteger((int)operand.TypeOf.IntWidth, signedness);
        }
    }

    /// <summary>
    /// Extends the <c>iN</c> on the IL stack from its logical width: sign-extends from bit N-1, or clears
    /// everything above bit N-1. Does nothing for N = 32, 64 or 128 (the whole stack value is the integer).
    /// </summary>
    private void EmitNormalizedInteger(int bits, Signedness signedness)
    {
        if (signedness == Signedness.Any || bits is 32 or 64 or 128)
        {
            return;
        }

        if (bits > 64)
        {
            // An Int128 on the stack: the same, with Int128 operators.
            switch (signedness)
            {
                case Signedness.Signed:
                    ILGenerator.Emit(OpCodes.Ldc_I4, 128 - bits);
                    ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LeftShift", [typeof(Int128), typeof(int)]));
                    ILGenerator.Emit(OpCodes.Ldc_I4, 128 - bits);
                    ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_RightShift", [typeof(Int128), typeof(int)]));
                    break;

                case Signedness.Unsigned:
                    EmitWideIntegerMask((uint)bits);
                    ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_BitwiseAnd", [typeof(Int128), typeof(Int128)]));
                    break;
            }

            return;
        }

        switch (signedness, bits)
        {
            case (Signedness.Signed, 8):
                ILGenerator.Emit(OpCodes.Conv_I1);
                break;

            case (Signedness.Signed, 16):
                ILGenerator.Emit(OpCodes.Conv_I2);
                break;

            case (Signedness.Signed, _):
                // The value is an int32 for containers up to 32 bits and an int64 above that.
                var shift = (TypeSystem.RoundUpToTypeSize(bits) == 64 ? 64 : 32) - bits;
                ILGenerator.Emit(OpCodes.Ldc_I4, shift);
                ILGenerator.Emit(OpCodes.Shl);
                ILGenerator.Emit(OpCodes.Ldc_I4, shift);
                ILGenerator.Emit(OpCodes.Shr);
                break;

            case (Signedness.Unsigned, 8):
                ILGenerator.Emit(OpCodes.Conv_U1);
                break;

            case (Signedness.Unsigned, 16):
                ILGenerator.Emit(OpCodes.Conv_U2);
                break;

            case (Signedness.Unsigned, _):
                if (TypeSystem.RoundUpToTypeSize(bits) == 64)
                {
                    ILGenerator.Emit(OpCodes.Ldc_I8, (1L << bits) - 1);
                }
                else
                {
                    ILGenerator.Emit(OpCodes.Ldc_I4, (1 << bits) - 1);
                }
                ILGenerator.Emit(OpCodes.And);
                break;
        }
    }

    /// <summary>How a comparison reads its operands. <c>eq</c>/<c>ne</c> only need both sides extended alike.</summary>
    private static Signedness GetSignedness(LLVMIntPredicate predicate) =>
        predicate is LLVMIntPredicate.LLVMIntSGE or LLVMIntPredicate.LLVMIntSGT
            or LLVMIntPredicate.LLVMIntSLE or LLVMIntPredicate.LLVMIntSLT
            ? Signedness.Signed
            : Signedness.Unsigned;

    private void EmitICmp(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);

        if (operand0.TypeOf.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            EmitVectorComparisonMask(instruction);
            EmitNarrowVectorMask(operand0.TypeOf, instruction.TypeOf);
            return;
        }

        var operandSignedness = GetSignedness(instruction.ICmpPredicate);
        EmitIntegerOperand(operand0, operandSignedness);
        EmitIntegerOperand(instruction.GetOperand(1), operandSignedness);

        switch (operand0.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind when operand0.TypeOf.IntWidth > 64:
                // Int128 is a struct — IL Ceq/Clt/Cgt don't work; use Int128/UInt128 operators.
                switch (instruction.ICmpPredicate)
                {
                    case LLVMIntPredicate.LLVMIntEQ:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Equality", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntNE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Inequality", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntSGE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_GreaterThanOrEqual", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntSGT:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_GreaterThan", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntSLE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LessThanOrEqual", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntSLT:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LessThan", [typeof(Int128), typeof(Int128)]));
                        break;

                    case LLVMIntPredicate.LLVMIntUGE:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UgeInt128)));
                        break;

                    case LLVMIntPredicate.LLVMIntUGT:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UgtInt128)));
                        break;

                    case LLVMIntPredicate.LLVMIntULE:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UleInt128)));
                        break;

                    case LLVMIntPredicate.LLVMIntULT:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UltInt128)));
                        break;

                    default:
                        throw new NotImplementedException($"Integer comparison predicate {instruction.ICmpPredicate} not implemented for i128: {instruction}");
                }
                break;

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

                    case LLVMIntPredicate.LLVMIntULE:
                        ILGenerator.Emit(OpCodes.Cgt_Un);
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Ceq);
                        break;

                    case LLVMIntPredicate.LLVMIntULT:
                        ILGenerator.Emit(OpCodes.Clt_Un);
                        break;

                    default:
                        throw new NotImplementedException($"Integer comparison predicate {instruction.ICmpPredicate} not implemented: {instruction}");
                }
                break;

            default:
                throw new NotImplementedException($"ICmp not implemented for type {operand0.TypeOf.Kind}: {instruction}");
        }
    }

    /// <summary>
    /// Emits a vector <c>icmp</c> or <c>fcmp</c> and leaves the full-width mask on the stack: a vector of
    /// integers as wide as the compared elements, with every bit of a lane set when the comparison holds.
    /// <see cref="EmitNarrowVectorMask"/> turns that into the <c>&lt;N x i1&gt;</c> value the rest of the emitter uses.
    /// </summary>
    private void EmitVectorComparisonMask(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);
        var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(operand0.TypeOf);

        // The BCL compares are typed: an unsigned comparison has to run on the unsigned vector type,
        // while the lanes themselves are signed (or nint) in our representation.
        var elementType = TypeSystem.GetMsilVectorElementType(operand0.TypeOf.ElementType);
        var compareElementType = elementType;
        var unsignedCompare = false;

        if (instruction.InstructionOpcode == LLVMOpcode.LLVMICmp)
        {
            var predicate = instruction.ICmpPredicate;
            unsignedCompare = predicate is LLVMIntPredicate.LLVMIntUGE or LLVMIntPredicate.LLVMIntUGT
                or LLVMIntPredicate.LLVMIntULE or LLVMIntPredicate.LLVMIntULT;

            var (vectorUtilityType, vectorComparisonMethodName) = predicate switch
            {
                LLVMIntPredicate.LLVMIntEQ => (nonGenericVectorType, nameof(Vector128.Equals)),
                LLVMIntPredicate.LLVMIntNE => (typeof(VectorUtility), $"Compare{nonGenericVectorType.Name}NotEquals"),
                LLVMIntPredicate.LLVMIntSGE or LLVMIntPredicate.LLVMIntUGE => (nonGenericVectorType, nameof(Vector128.GreaterThanOrEqual)),
                LLVMIntPredicate.LLVMIntSGT or LLVMIntPredicate.LLVMIntUGT => (nonGenericVectorType, nameof(Vector128.GreaterThan)),
                LLVMIntPredicate.LLVMIntSLE or LLVMIntPredicate.LLVMIntULE => (nonGenericVectorType, nameof(Vector128.LessThanOrEqual)),
                LLVMIntPredicate.LLVMIntSLT or LLVMIntPredicate.LLVMIntULT => (nonGenericVectorType, nameof(Vector128.LessThan)),
                _ => throw new NotImplementedException($"Integer comparison predicate {predicate} not implemented for vectors: {instruction}"),
            };

            var operandSignedness = GetSignedness(predicate);
            if (unsignedCompare)
            {
                var (asUnsignedName, unsignedElementType) = GetUnsignedVectorCast(operand0.TypeOf.ElementType);
                compareElementType = unsignedElementType;
                var asUnsigned = nonGenericVectorType.GetMethodStrict(asUnsignedName).MakeGenericMethod(elementType);

                EmitIntegerOperand(operand0, operandSignedness);
                ILGenerator.Emit(OpCodes.Call, asUnsigned);
                EmitIntegerOperand(instruction.GetOperand(1), operandSignedness);
                ILGenerator.Emit(OpCodes.Call, asUnsigned);
            }
            else
            {
                EmitIntegerOperand(operand0, operandSignedness);
                EmitIntegerOperand(instruction.GetOperand(1), operandSignedness);
            }

            var genericVectorMethod = vectorUtilityType.GetStaticMethodStrict(vectorComparisonMethodName);
            ILGenerator.Emit(OpCodes.Call, genericVectorMethod.MakeGenericMethod(compareElementType));
        }
        else
        {
            EmitVectorFloatComparison(instruction, nonGenericVectorType, elementType);
        }

        // If result is not an integer type, bitcast it to integer type.
        switch (operand0.TypeOf.ElementType.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsInt64)).MakeGenericMethod(compareElementType));
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsInt32)).MakeGenericMethod(compareElementType));
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                if (unsignedCompare)
                {
                    // Back to the signed vector of the same width.
                    var asSignedName = TypeSystem.RoundUpToTypeSize(TypeSystem.GetSizeOfTypeInBits(operand0.TypeOf.ElementType)) switch
                    {
                        8 => nameof(Vector128.AsSByte),
                        16 => nameof(Vector128.AsInt16),
                        32 => nameof(Vector128.AsInt32),
                        64 => nameof(Vector128.AsInt64),
                        var bits => throw new NotImplementedException($"Unsigned vector comparison of {bits}-bit lanes: {instruction}"),
                    };
                    ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(asSignedName).MakeGenericMethod(compareElementType));
                }
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                // nint, nuint and long are all pointer-sized; reinterpret so the result is Vector<long>.
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsInt64)).MakeGenericMethod(compareElementType));
                break;

            default:
                throw new NotImplementedException($"Vector comparison not implemented for element type {operand0.TypeOf.ElementType.Kind}: {instruction}");
        }
    }

    /// <summary>
    /// Emits a vector <c>fcmp</c> and leaves the floating-point vector of lanes that are all ones where the
    /// predicate holds and all zeros elsewhere, for all 16 predicates. The BCL compares are the ordered ones
    /// (false when a lane is NaN), so each unordered predicate is the complement of the opposite ordered
    /// comparison: <c>ugt</c> is <c>!(a &lt;= b)</c>. <c>one</c> is <c>a &lt; b | a &gt; b</c>, and <c>ord</c> is
    /// <c>a == a &amp; b == b</c>.
    /// </summary>
    private void EmitVectorFloatComparison(LLVMValueRef instruction, Type nonGenericVectorType, Type laneType)
    {
        var operand0 = instruction.GetOperand(0);
        var vectorType = TypeSystem.GetMsilVectorType(operand0.TypeOf);

        var left = ILGenerator.DeclareLocal(vectorType);
        var right = ILGenerator.DeclareLocal(vectorType);
        EmitValue(operand0);
        EmitValue(instruction.GetOperand(1));
        ILGenerator.Emit(OpCodes.Stloc, right);
        ILGenerator.Emit(OpCodes.Stloc, left);

        void Compare(string methodName, LocalBuilder first, LocalBuilder second, Type? declaringType = null)
        {
            ILGenerator.Emit(OpCodes.Ldloc, first);
            ILGenerator.Emit(OpCodes.Ldloc, second);
            ILGenerator.Emit(OpCodes.Call, (declaringType ?? nonGenericVectorType).GetStaticMethodStrict(methodName).MakeGenericMethod(laneType));
        }

        void Combine(string methodName)
        {
            ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetStaticMethodStrict(methodName).MakeGenericMethod(laneType));
        }

        void OrderedNotEqual()
        {
            Compare(nameof(Vector128.LessThan), left, right);
            Compare(nameof(Vector128.GreaterThan), left, right);
            Combine(nameof(Vector128.BitwiseOr));
        }

        void Ordered()
        {
            Compare(nameof(Vector128.Equals), left, left);
            Compare(nameof(Vector128.Equals), right, right);
            Combine(nameof(Vector128.BitwiseAnd));
        }

        switch (instruction.FCmpPredicate)
        {
            case LLVMRealPredicate.LLVMRealPredicateFalse:
                ILGenerator.Emit(OpCodes.Call, vectorType.GetMethodStrict("get_Zero"));
                break;

            case LLVMRealPredicate.LLVMRealPredicateTrue:
                ILGenerator.Emit(OpCodes.Call, vectorType.GetMethodStrict("get_AllBitsSet"));
                break;

            case LLVMRealPredicate.LLVMRealOEQ: Compare(nameof(Vector128.Equals), left, right); break;
            case LLVMRealPredicate.LLVMRealOGT: Compare(nameof(Vector128.GreaterThan), left, right); break;
            case LLVMRealPredicate.LLVMRealOGE: Compare(nameof(Vector128.GreaterThanOrEqual), left, right); break;
            case LLVMRealPredicate.LLVMRealOLT: Compare(nameof(Vector128.LessThan), left, right); break;
            case LLVMRealPredicate.LLVMRealOLE: Compare(nameof(Vector128.LessThanOrEqual), left, right); break;
            case LLVMRealPredicate.LLVMRealONE: OrderedNotEqual(); break;
            case LLVMRealPredicate.LLVMRealORD: Ordered(); break;

            case LLVMRealPredicate.LLVMRealUGT: Compare(nameof(Vector128.LessThanOrEqual), left, right); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealUGE: Compare(nameof(Vector128.LessThan), left, right); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealULT: Compare(nameof(Vector128.GreaterThanOrEqual), left, right); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealULE: Compare(nameof(Vector128.GreaterThan), left, right); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealUEQ: OrderedNotEqual(); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealUNO: Ordered(); Combine(nameof(Vector128.OnesComplement)); break;
            case LLVMRealPredicate.LLVMRealUNE:
                Compare($"Compare{nonGenericVectorType.Name}NotEquals", left, right, typeof(VectorUtility));
                break;

            default:
                throw new NotImplementedException($"Float comparison predicate {instruction.FCmpPredicate} not implemented for vectors: {instruction}");
        }
    }

    /// <summary>
    /// The <c>Vector128.AsXxx</c> method that reinterprets a vector of <paramref name="laneType"/> lanes as
    /// unsigned, and the unsigned lane type it gives.
    /// </summary>
    private (string MethodName, Type UnsignedElementType) GetUnsignedVectorCast(LLVMTypeRef laneType)
    {
        if (laneType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            return (nameof(Vector128.AsNUInt), typeof(nuint));
        }

        return TypeSystem.RoundUpToTypeSize(TypeSystem.GetSizeOfTypeInBits(laneType)) switch
        {
            8 => (nameof(Vector128.AsByte), typeof(byte)),
            16 => (nameof(Vector128.AsUInt16), typeof(ushort)),
            32 => (nameof(Vector128.AsUInt32), typeof(uint)),
            64 => (nameof(Vector128.AsUInt64), typeof(ulong)),
            var bits => throw new NotImplementedException($"Unsigned vector comparison of {bits}-bit lanes"),
        };
    }

    /// <summary>
    /// Narrows a full-width mask (lanes as wide as those of <paramref name="comparedType"/>, all ones or all zeros),
    /// such as <see cref="EmitVectorComparisonMask"/> leaves on the stack, to the <c>&lt;N x i1&gt;</c>
    /// representation <paramref name="maskType"/>.
    /// </summary>
    private void EmitNarrowVectorMask(LLVMTypeRef comparedType, LLVMTypeRef maskType)
    {
        // Save intermediate result to local, since we'll need to reference it multiple times below.
        var integerVectorType = TypeSystem.GetGenericVectorType(comparedType).MakeGenericType(TypeSystem.GetIntegerType(TypeSystem.GetSizeOfTypeInBits(comparedType.ElementType)));
        var intermediateLocal = ILGenerator.DeclareLocal(integerVectorType);
        ILGenerator.Emit(OpCodes.Stloc, intermediateLocal);

        // We need to truncate the result from e.g. Vector128<long> to Vector64<sbyte>,
        // something like this:
        // Vector64<sbyte>.Zero.WithElement(0, (sbyte)input[0]).WithElement(1, (sbyte)input[1]);
        var resultVectorType = TypeSystem.GetMsilVectorType(maskType);
        ILGenerator.Emit(OpCodes.Call, resultVectorType.GetMethodStrict("get_Zero"));

        for (var i = 0; i < maskType.VectorSize; i++)
        {
            ILGenerator.Emit(OpCodes.Ldc_I4, i);
            ILGenerator.Emit(OpCodes.Ldloca, intermediateLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, i);
            ILGenerator.Emit(OpCodes.Call, integerVectorType.GetMethodStrict("get_Item"));

            // Cast to `sbyte`.
            ILGenerator.Emit(OpCodes.Conv_I1);

            EmitVectorWithElement(
                TypeSystem.GetNonGenericVectorType(maskType),
                typeof(sbyte));
        }
    }

    private void EmitFCmp(LLVMValueRef instruction)
    {
        var operand0 = instruction.GetOperand(0);

        if (operand0.TypeOf.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            EmitVectorComparisonMask(instruction);
            EmitNarrowVectorMask(operand0.TypeOf, instruction.TypeOf);
            return;
        }

        EmitValue(operand0);
        EmitValue(instruction.GetOperand(1));

        switch (operand0.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
            case LLVMTypeKind.LLVMFloatTypeKind:
                EmitFloatComparison(instruction.FCmpPredicate, operand0.TypeOf);
                break;

            default:
                throw new NotImplementedException($"FCmp not implemented for type {operand0.TypeOf.Kind}: {instruction}");
        }
    }

    /// <summary>
    /// How an operation reads an integer operand. An <c>iN</c> narrower than its container, or whose
    /// container is narrower than 32 bits, is only defined in its low N bits: whatever produced it
    /// (<c>load</c>, <c>trunc</c>, <c>add</c>, …) may have left anything above them. Every consumer that
    /// depends on the upper bits therefore extends its operands itself (<see cref="EmitIntegerOperand"/>).
    /// </summary>
    private enum Signedness
    {
        /// <summary>The operation's low N bits don't depend on the operand's upper bits (<c>add</c>, <c>and</c>, …).</summary>
        Any,
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
                    EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Signed);
                }
                if (opcode == LLVMOpcode.LLVMUIToFP)
                {
                    EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Unsigned);
                    ILGenerator.Emit(OpCodes.Conv_R_Un);
                }
                ILGenerator.Emit(OpCodes.Conv_R8);
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                // Ensure source type is treated as signed.
                if (opcode == LLVMOpcode.LLVMSIToFP)
                {
                    EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Signed);
                }
                if (opcode == LLVMOpcode.LLVMUIToFP)
                {
                    EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Unsigned);
                    ILGenerator.Emit(OpCodes.Conv_R_Un);
                }
                ILGenerator.Emit(OpCodes.Conv_R4);
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                // Extend the source from its logical width before it is widened.
                if (fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    switch (opcode)
                    {
                        case LLVMOpcode.LLVMSExt:
                            EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Signed);
                            break;

                        case LLVMOpcode.LLVMZExt:
                            EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Unsigned);
                            break;
                    }
                }

                // Between two wide integers the Int128 on the stack is already the result.
                var fromWide = fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && fromType.IntWidth > 64;
                if (fromWide && toType.IntWidth > 64)
                {
                    break;
                }

                // For narrowing from a wide integer (stored as Int128), extract the lower 64 bits first.
                if (fromWide)
                {
                    ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.Int128ToLong)));
                }

                EmitIntegerConversion((int)toType.IntWidth, signedness);
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                // inttoptr zero-extends its operand.
                if (opcode == LLVMOpcode.LLVMIntToPtr && fromType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    EmitNormalizedInteger((int)fromType.IntWidth, Signedness.Unsigned);
                    ILGenerator.Emit(OpCodes.Conv_U);
                }
                else
                {
                    ILGenerator.Emit(OpCodes.Conv_I);
                }
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                if (opcode == LLVMOpcode.LLVMTrunc && IsMaskType(toType))
                {
                    EmitTruncToMask(fromType, toType);
                    break;
                }

                // The ZExt helpers extend an i8 lane. An i1 lane is only defined in its low bit (a true lane
                // is all ones), so clear the rest first.
                if (opcode == LLVMOpcode.LLVMZExt
                    && fromType.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                    && fromType.ElementType.IntWidth == 1)
                {
                    ILGenerator.Emit(
                        OpCodes.Call,
                        typeof(VectorUtility).GetMethodStrict(nameof(VectorUtility.NormalizeI1Lanes), [TypeSystem.GetMsilVectorType(fromType)]));
                }

                if (fromType.ElementType.Kind == LLVMTypeKind.LLVMPointerTypeKind || toType.ElementType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    // inttoptr and ptrtoint of vectors have a helper per shape.
                    var convertMethodPrefix = opcode.ToString().Remove(0, 4);
                    var convertMethodName = $"{convertMethodPrefix}{GetIntrinsicMethodSuffix(fromType)}To{GetIntrinsicMethodSuffix(toType)}";
                    ILGenerator.Emit(OpCodes.Call, typeof(VectorUtility).GetStaticMethodStrict(convertMethodName));
                }
                else
                {
                    EmitVectorLaneConversion(opcode, fromType, toType);
                }
                break;

            default:
                throw new NotImplementedException($"Conversion not implemented to {toType} for operand {operand}: {opcode}");
        }
    }

    private static bool IsMaskType(LLVMTypeRef type) =>
        type.Kind == LLVMTypeKind.LLVMVectorTypeKind
        && type.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind
        && type.ElementType.IntWidth == 1;

    /// <summary>
    /// <c>trunc &lt;N x iM&gt; to &lt;N x i1&gt;</c> keeps the low bit of each lane. That is the comparison
    /// <c>(x &amp; 1) == 1</c>, so it produces the mask the same way a comparison does:
    /// <c>Vector128.Equals(Vector128.BitwiseAnd(x, Vector128.Create(1)), Vector128.Create(1))</c>.
    /// </summary>
    private void EmitTruncToMask(LLVMTypeRef fromType, LLVMTypeRef maskType)
    {
        var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(fromType);
        var laneType = TypeSystem.GetMsilVectorElementType(fromType.ElementType);
        var createOne = nonGenericVectorType.GetMethodStrict(nameof(Vector128.Create), [laneType]);

        void EmitOnes()
        {
            if (laneType == typeof(long) || laneType == typeof(ulong))
            {
                ILGenerator.Emit(OpCodes.Ldc_I8, 1L);
            }
            else
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_1);
            }

            ILGenerator.Emit(OpCodes.Call, createOne);
        }

        EmitOnes();
        ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetStaticMethodStrict(nameof(Vector128.BitwiseAnd)).MakeGenericMethod(laneType));
        EmitOnes();
        ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetStaticMethodStrict(nameof(Vector128.Equals)).MakeGenericMethod(laneType));
        EmitNarrowVectorMask(fromType, maskType);
    }

    /// <summary>
    /// Converts the value on the IL stack (an int32, an int64 or a floating-point number) to the container
    /// of an <c>iN</c>. This only changes the stack type: bits above N are left as they are, because
    /// whatever reads the result extends it itself (<see cref="EmitIntegerOperand"/>).
    /// </summary>
    private void EmitIntegerConversion(int bits, Signedness signedness)
    {
        switch (TypeSystem.RoundUpToTypeSize(bits), signedness)
        {
            case (8 or 16 or 32, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I4);
                break;

            case (8 or 16 or 32, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U4);
                break;

            case (64, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I8);
                break;

            case (64, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U8);
                break;

            case (128, Signedness.Signed):
                ILGenerator.Emit(OpCodes.Conv_I8);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Implicit", [typeof(long)]));
                break;

            case (128, Signedness.Unsigned):
                ILGenerator.Emit(OpCodes.Conv_U8);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Implicit", [typeof(ulong)]));
                break;

            default:
                throw new NotImplementedException($"Integer conversion not implemented to int width {bits} {signedness}");
        }
    }

    /// <summary>
    /// The distinct shift counts of a constant vector, ignoring <c>undef</c> and <c>poison</c> lanes, or
    /// <see langword="null"/> when <paramref name="operand"/> is not a constant vector.
    /// </summary>
    private static List<int>? GetConstantShiftCounts(LLVMValueRef operand)
    {
        switch (operand.Kind)
        {
            case LLVMValueKind.LLVMConstantAggregateZeroValueKind:
                return [0];

            case LLVMValueKind.LLVMConstantDataVectorValueKind:
            case LLVMValueKind.LLVMConstantVectorValueKind:
                return Enumerable.Range(0, (int)operand.TypeOf.VectorSize)
                    .Select(j => operand.GetAggregateElement((uint)j))
                    .Where(element => element.Kind == LLVMValueKind.LLVMConstantIntValueKind)
                    .Select(element => (int)element.ConstIntSExt)
                    .Distinct()
                    .ToList();

            default:
                return null;
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

        if (instruction.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && instruction.TypeOf.IntWidth > 64)
        {
            EmitWideIntegerBinaryOp(instruction);
            return;
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
                            // Amount to shift by. A constant with the same value in every defined lane (undef lanes
                            // don't count) is a scalar count; anything else is a vector of counts.
                            var counts = GetConstantShiftCounts(operand);
                            if (counts is { Count: <= 1 })
                            {
                                ILGenerator.Emit(OpCodes.Ldc_I4, counts.FirstOrDefault());
                            }
                            else
                            {
                                isVectorShiftWithVectorCount = true;
                                EmitValue(operand);
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
                // A shift amount is read as unsigned whatever the shift is.
                var isShiftAmount = i == 1 && instruction.InstructionOpcode is LLVMOpcode.LLVMShl or LLVMOpcode.LLVMLShr or LLVMOpcode.LLVMAShr;
                EmitIntegerOperand(operand, isShiftAmount ? Signedness.Unsigned : signedness);
            }
        }

        switch (instruction.TypeOf.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(scalarOpCode);
                break;

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
                            ILGenerator.Emit(OpCodes.Ldc_I4, (int)instruction.TypeOf.VectorSize);
                            vectorMethod = typeof(VectorUtility).GetStaticMethodStrict($"Variable{vectorMethodName}")
                                .MakeGenericMethod(vectorType, TypeSystem.GetMsilVectorElementType(instruction.TypeOf.ElementType));
                        }
                        else
                        {
                            vectorMethod = nonGenericVectorType.GetMethodStrict(vectorMethodName, [vectorType, typeof(int)]);
                        }
                        break;

                    case "SignedRemainder":
                    case "UnsignedRemainder":
                        // The lanes of an unsigned remainder are read as unsigned, though the vector is signed.
                        var signedLaneType = TypeSystem.GetMsilVectorElementType(instruction.TypeOf.ElementType);
                        var remainderLaneType = vectorMethodName == "UnsignedRemainder" ? GetUnsignedLaneType(signedLaneType) : signedLaneType;
                        ILGenerator.Emit(OpCodes.Ldc_I4, (int)instruction.TypeOf.VectorSize);
                        vectorMethod = typeof(VectorUtility).GetStaticMethodStrict(nameof(VectorUtility.Remainder))
                            .MakeGenericMethod(TypeSystem.GetMsilVectorType(instruction.TypeOf), remainderLaneType);
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

    private void EmitWideIntegerBinaryOp(LLVMValueRef instruction)
    {
        var opcode = instruction.InstructionOpcode;

        var lhs = instruction.GetOperand(0);
        var rhs = instruction.GetOperand(1);

        switch (opcode)
        {
            case LLVMOpcode.LLVMLShr:
                // Use helper for logical (unsigned) right shift via UInt128.
                EmitIntegerOperand(lhs, Signedness.Unsigned);
                EmitShiftAmount(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.LshrInt128)));
                break;

            case LLVMOpcode.LLVMAShr:
                EmitIntegerOperand(lhs, Signedness.Signed);
                EmitShiftAmount(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_RightShift", [typeof(Int128), typeof(int)]));
                break;

            case LLVMOpcode.LLVMShl:
                EmitValue(lhs);
                EmitShiftAmount(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LeftShift", [typeof(Int128), typeof(int)]));
                break;

            case LLVMOpcode.LLVMAdd:
            case LLVMOpcode.LLVMFAdd:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Addition", [typeof(Int128), typeof(Int128)]));
                break;

            case LLVMOpcode.LLVMSub:
            case LLVMOpcode.LLVMFSub:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Subtraction", [typeof(Int128), typeof(Int128)]));
                break;

            case LLVMOpcode.LLVMMul:
            case LLVMOpcode.LLVMFMul:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Multiply", [typeof(Int128), typeof(Int128)]));
                break;

            case LLVMOpcode.LLVMAnd:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_BitwiseAnd", [typeof(Int128), typeof(Int128)]));
                break;

            case LLVMOpcode.LLVMOr:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_BitwiseOr", [typeof(Int128), typeof(Int128)]));
                break;

            case LLVMOpcode.LLVMXor:
                EmitValue(lhs);
                EmitValue(rhs);
                ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_ExclusiveOr", [typeof(Int128), typeof(Int128)]));
                break;

            default:
                throw new NotImplementedException($"Wide integer binary op {opcode} not implemented");
        }
    }

    private void EmitShiftAmount(LLVMValueRef operand)
    {
        // Shift amount must be an int for Int128/UInt128 shift operators.
        if (operand.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
            operand.TypeOf.IntWidth > 64)
        {
            // Wide integer shift count: emit directly as i32 to avoid pushing an Int128 struct.
            if (operand.Kind == LLVMValueKind.LLVMConstantIntValueKind)
            {
                ILGenerator.Emit(OpCodes.Ldc_I4, (int)operand.ConstIntSExt);
            }
            else
            {
                EmitIntegerOperand(operand, Signedness.Unsigned);
                ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.Int128ToLong)));
                ILGenerator.Emit(OpCodes.Conv_I4);
            }
            return;
        }

        EmitIntegerOperand(operand, Signedness.Unsigned);
        if (operand.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
            TypeSystem.RoundUpToTypeSize((int)operand.TypeOf.IntWidth) != 32)
        {
            ILGenerator.Emit(OpCodes.Conv_I4);
        }
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
            var operandSignedness = GetSignedness(condition.ICmpPredicate);
            EmitIntegerOperand(condition.GetOperand(0), operandSignedness);
            EmitIntegerOperand(condition.GetOperand(1), operandSignedness);

            // Int128 is a struct — Beq/Blt/etc. don't work; use Int128/UInt128 operators then Brtrue.
            if (condition.GetOperand(0).TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
                condition.GetOperand(0).TypeOf.IntWidth > 64)
            {
                switch (condition.ICmpPredicate)
                {
                    case LLVMIntPredicate.LLVMIntEQ:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Equality", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntNE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_Inequality", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntSGE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_GreaterThanOrEqual", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntSGT:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_GreaterThan", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntSLE:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LessThanOrEqual", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntSLT:
                        ILGenerator.Emit(OpCodes.Call, typeof(Int128).GetMethodStrict("op_LessThan", [typeof(Int128), typeof(Int128)]));
                        break;
                    case LLVMIntPredicate.LLVMIntUGE:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UgeInt128)));
                        break;
                    case LLVMIntPredicate.LLVMIntUGT:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UgtInt128)));
                        break;
                    case LLVMIntPredicate.LLVMIntULE:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UleInt128)));
                        break;
                    case LLVMIntPredicate.LLVMIntULT:
                        ILGenerator.Emit(OpCodes.Call, typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.UltInt128)));
                        break;
                    default:
                        throw new NotImplementedException($"Branch condition i128 comparison {condition.ICmpPredicate} not implemented: {condition}");
                }
                return OpCodes.Brtrue;
            }

            return condition.ICmpPredicate switch
            {
                LLVMIntPredicate.LLVMIntEQ => OpCodes.Beq,
                LLVMIntPredicate.LLVMIntNE => OpCodes.Bne_Un,
                LLVMIntPredicate.LLVMIntSGE => OpCodes.Bge,
                LLVMIntPredicate.LLVMIntSGT => OpCodes.Bgt,
                LLVMIntPredicate.LLVMIntSLT => OpCodes.Blt,
                LLVMIntPredicate.LLVMIntSLE => OpCodes.Ble,
                LLVMIntPredicate.LLVMIntUGE => OpCodes.Bge_Un,
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
            var op0 = condition.GetOperand(0);
            EmitValue(op0);
            EmitValue(condition.GetOperand(1));

            // A predicate with a single branch opcode is fused. The others compute the i1 and branch on it,
            // so every predicate that `EmitFloatComparison` handles can be a branch condition.
            var branchOpCode = GetFloatBranchOpCode(condition.FCmpPredicate);
            if (branchOpCode != null)
            {
                return branchOpCode.Value;
            }

            EmitFloatComparison(condition.FCmpPredicate, op0.TypeOf);
            return OpCodes.Brtrue;
        }
        else
        {
            // An i1 is true when its low bit is set, whatever is above it.
            EmitIntegerOperand(condition, Signedness.Unsigned);

            return OpCodes.Brtrue;
        }
    }

    /// <summary>
    /// The branch opcode that tests <paramref name="predicate"/> on the two operands on the stack, if there
    /// is one. `beq`, `bgt`, `bge`, `blt` and `ble` are false when an operand is NaN, and their `.un` forms
    /// (and `bne.un`) are true: they are the ordered and the unordered predicates respectively.
    /// </summary>
    private static OpCode? GetFloatBranchOpCode(LLVMRealPredicate predicate) => predicate switch
    {
        LLVMRealPredicate.LLVMRealOEQ => OpCodes.Beq,
        LLVMRealPredicate.LLVMRealOGT => OpCodes.Bgt,
        LLVMRealPredicate.LLVMRealOGE => OpCodes.Bge,
        LLVMRealPredicate.LLVMRealOLT => OpCodes.Blt,
        LLVMRealPredicate.LLVMRealOLE => OpCodes.Ble,
        LLVMRealPredicate.LLVMRealUGT => OpCodes.Bgt_Un,
        LLVMRealPredicate.LLVMRealUGE => OpCodes.Bge_Un,
        LLVMRealPredicate.LLVMRealULT => OpCodes.Blt_Un,
        LLVMRealPredicate.LLVMRealULE => OpCodes.Ble_Un,
        LLVMRealPredicate.LLVMRealUNE => OpCodes.Bne_Un,
        _ => null,
    };

    /// <summary>
    /// Replaces the two floating-point operands on the stack with the result of <paramref name="predicate"/>
    /// (an int32 that is 0 or 1), for all 16 predicates. `ceq`, `cgt` and `clt` are false when an operand is
    /// NaN and `cgt.un` and `clt.un` are true, so each comparison that has to be true or false for NaN is the
    /// opposite comparison negated: `a >= b` (ordered) is `!(a &lt; b unordered)`, `a >= b` (unordered) is `!(a &lt; b)`.
    /// </summary>
    private void EmitFloatComparison(LLVMRealPredicate predicate, LLVMTypeRef operandType)
    {
        switch (predicate)
        {
            case LLVMRealPredicate.LLVMRealPredicateFalse:
            case LLVMRealPredicate.LLVMRealPredicateTrue:
                ILGenerator.Emit(OpCodes.Pop);
                ILGenerator.Emit(OpCodes.Pop);
                ILGenerator.Emit(predicate == LLVMRealPredicate.LLVMRealPredicateTrue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                return;

            case LLVMRealPredicate.LLVMRealOEQ: ILGenerator.Emit(OpCodes.Ceq); return;
            case LLVMRealPredicate.LLVMRealOGT: ILGenerator.Emit(OpCodes.Cgt); return;
            case LLVMRealPredicate.LLVMRealOLT: ILGenerator.Emit(OpCodes.Clt); return;
            case LLVMRealPredicate.LLVMRealUGT: ILGenerator.Emit(OpCodes.Cgt_Un); return;
            case LLVMRealPredicate.LLVMRealULT: ILGenerator.Emit(OpCodes.Clt_Un); return;

            case LLVMRealPredicate.LLVMRealOGE: EmitNegatedComparison(OpCodes.Clt_Un); return;
            case LLVMRealPredicate.LLVMRealOLE: EmitNegatedComparison(OpCodes.Cgt_Un); return;
            case LLVMRealPredicate.LLVMRealUGE: EmitNegatedComparison(OpCodes.Clt); return;
            case LLVMRealPredicate.LLVMRealULE: EmitNegatedComparison(OpCodes.Cgt); return;
            case LLVMRealPredicate.LLVMRealUNE: EmitNegatedComparison(OpCodes.Ceq); return;
        }

        var floatType = TypeSystem.GetMsilType(operandType);
        var helperName = predicate switch
        {
            LLVMRealPredicate.LLVMRealONE or LLVMRealPredicate.LLVMRealUEQ => nameof(LLVMIntrinsics.AreOrderedAndNotEqual),
            LLVMRealPredicate.LLVMRealORD or LLVMRealPredicate.LLVMRealUNO => nameof(LLVMIntrinsics.AreUnordered),
            _ => throw new NotImplementedException($"Float comparison predicate {predicate} not implemented"),
        };
        ILGenerator.Emit(OpCodes.Call, typeof(LLVMIntrinsics).GetMethodStrict(helperName, [floatType, floatType]));

        // The helpers are the true result of `one` and `uno`; `ueq` and `ord` are their opposites.
        if (predicate is LLVMRealPredicate.LLVMRealUEQ or LLVMRealPredicate.LLVMRealORD)
        {
            ILGenerator.Emit(OpCodes.Ldc_I4_0);
            ILGenerator.Emit(OpCodes.Ceq);
        }
    }

    private void EmitNegatedComparison(OpCode comparison)
    {
        ILGenerator.Emit(comparison);
        ILGenerator.Emit(OpCodes.Ldc_I4_0);
        ILGenerator.Emit(OpCodes.Ceq);
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
            var parameters = functionType.GetParamTypes();
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

        // Special handling for printf/fprintf/__sprintf_chk: variadic calling convention doesn't work on non-Windows.
        // We implement them in managed code by parsing the format string + a call to Console.Write / Marshal.Copy.
        if (isVarArg && functionToCall.Name is "printf" or "fprintf" or "__sprintf_chk")
        {
            var fixedParamTypes = functionType.GetParamTypes().Select(t => TypeSystem.GetMsilType(t)).ToArray();
            var allParamTypes = fixedParamTypes.Concat(varArgsParameterTypes).ToArray();
            var overload = functionToCall.Name switch
            {
                "fprintf" => CompiledModule.GetOrCreateFprintfOverload(allParamTypes),
                "__sprintf_chk" => CompiledModule.GetOrCreateSprintfChkOverload(allParamTypes),
                _ => CompiledModule.GetOrCreatePrintfOverload(allParamTypes),
            };
            ILGenerator.Emit(OpCodes.Call, overload);
            return;
        }

        ILGenerator.EmitCall(
            OpCodes.Call,
            method,
            varArgsParameterTypes);
    }

    private unsafe void HandleDebugDeclare(LLVMValueRef instruction)
    {
        var value = instruction.GetOperand(0).GetMDNodeOperands()[0];

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
        var firstIndex = instruction.GetOperand(1);

        // When the first index is a vector, the result is a vector of pointers.
        if (firstIndex.TypeOf.Kind == LLVMTypeKind.LLVMVectorTypeKind)
        {
            EmitVectorGetElementPtr(instruction, pointer, firstIndex);
            return;
        }

        EmitValue(pointer);

        var sourceElementType = (LLVMTypeRef)LLVM.GetGEPSourceElementType(instruction);
        var currentType = sourceElementType;

        // First index operand always indexes into the source element pointer type.
        EmitIndexedPtr(firstIndex, currentType);

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

    private unsafe void EmitVectorGetElementPtr(LLVMValueRef instruction, LLVMValueRef pointer, LLVMValueRef indexVector)
    {
        var sourceElementType = (LLVMTypeRef)LLVM.GetGEPSourceElementType(instruction);
        var sizeInBytes = TypeSystem.GetSizeOfTypeInBytes(sourceElementType);
        var vectorSize = (int)indexVector.TypeOf.VectorSize;

        // Store base pointer to a local so we can reuse it.
        var baseLocal = ILGenerator.DeclareLocal(typeof(void*));
        EmitValue(pointer);
        ILGenerator.Emit(OpCodes.Stloc, baseLocal);

        // Store the index vector to a local so we can extract individual elements.
        var indexVectorLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(indexVector.TypeOf));
        EmitValue(indexVector);
        ILGenerator.Emit(OpCodes.Stloc, indexVectorLocal);

        // Create a zero-initialised result vector local.
        var resultType = TypeSystem.GetMsilType(instruction.TypeOf);
        var resultLocal = ILGenerator.DeclareLocal(resultType);
        ILGenerator.Emit(OpCodes.Ldloca, resultLocal);
        ILGenerator.Emit(OpCodes.Initobj, resultType);

        var indexElementSizeInBytes = TypeSystem.GetSizeOfTypeInBytes(indexVector.TypeOf.ElementType);
        var resultElementSizeInBytes = TypeSystem.GetSizeOfTypeInBytes(instruction.TypeOf.ElementType);

        for (var i = 0; i < vectorSize; i++)
        {
            // Push address of resultLocal[i].
            ILGenerator.Emit(OpCodes.Ldloca, resultLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, i * resultElementSizeInBytes);
            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);

            // Compute base + indices[i] * sizeInBytes.
            ILGenerator.Emit(OpCodes.Ldloc, baseLocal);

            ILGenerator.Emit(OpCodes.Ldloca, indexVectorLocal);
            ILGenerator.Emit(OpCodes.Ldc_I4, i * indexElementSizeInBytes);
            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);
            EmitLoadIndirect(indexVector.TypeOf.ElementType);
            ILGenerator.Emit(OpCodes.Conv_I8);

            if (sizeInBytes != 1)
            {
                ILGenerator.Emit(OpCodes.Ldc_I8, (long)sizeInBytes);
                ILGenerator.Emit(OpCodes.Mul);
            }

            ILGenerator.Emit(OpCodes.Conv_U);
            ILGenerator.Emit(OpCodes.Add);

            // Store the computed pointer into resultLocal[i].
            ILGenerator.Emit(OpCodes.Stind_I);
        }

        ILGenerator.Emit(OpCodes.Ldloc, resultLocal);
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
            // A GEP index is signed whatever its width.
            EmitIntegerOperand(index, Signedness.Signed);

            if (index.TypeOf.IntWidth < 64)
            {
                ILGenerator.Emit(OpCodes.Conv_I8);
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
                var nonGenericVectorType = TypeSystem.GetNonGenericVectorType(operand1.TypeOf);

                if (CanSelectOnComparisonMask(operand0, operand1))
                {
                    // The condition is a vector comparison used only here, so use the full-width mask it
                    // produces as it is, instead of narrowing it to <N x i1> and widening it again:
                    //
                    // LLVM:
                    // %c = icmp slt <4 x i32> %a, %b
                    // %r = select <4 x i1> %c, <4 x i32> %x, <4 x i32> %y
                    //
                    // .NET:
                    // Vector128.ConditionalSelect(Vector128.LessThan(a, b), x, y)
                    EmitVectorComparisonMask(operand0);
                    TryResizeVectorMask(
                        GetMaskLaneBits(operand0.GetOperand(0).TypeOf.ElementType),
                        GetMaskLaneBits(operand1.TypeOf.ElementType),
                        (int)operand1.TypeOf.VectorSize,
                        emit: true);
                    EmitIntegerVectorAsElementType(operand1.TypeOf.ElementType, nonGenericVectorType);
                }
                else
                {
                    EmitValue(operand0);

                    // Now we need to convert condition from <2 x i1> to same type as left and right operands.

                    // Store to local because we'll need to reference it multiple times below.
                    var intermediateLocal = ILGenerator.DeclareLocal(TypeSystem.GetMsilVectorType(operand0.TypeOf));
                    ILGenerator.Emit(OpCodes.Stloc, intermediateLocal);

                    // We need to extend the result from e.g. Vector64<sbyte> to Vector64<int>,
                    // something like this:
                    // Vector64.Create((int)input[0], (int)input[1]);
                    // The padding lanes of a vector of 16 or 32 bits are zero.

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

                    var containerLaneCount = TypeSystem.GetVectorContainerLaneCount(operand1.TypeOf);
                    for (var i = (int)instruction.TypeOf.VectorSize; i < containerLaneCount; i++)
                    {
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                    }

                    var createMethodArgumentTypes = new Type[containerLaneCount];
                    // Integer type with same bitwidth as operand1 element type.
                    Array.Fill(createMethodArgumentTypes, TypeSystem.GetIntegerType(TypeSystem.RoundUpToTypeSize(operand1ElementSizeInBits)));
                    var createMethod = nonGenericVectorType.GetMethodStrict(nameof(Vector128.Create), createMethodArgumentTypes);
                    ILGenerator.Emit(OpCodes.Call, createMethod);

                    EmitIntegerVectorAsElementType(operand1.TypeOf.ElementType, nonGenericVectorType);
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

    /// <summary>
    /// Whether a vector <c>select</c> can use the full-width mask of its condition directly: the condition is a
    /// vector comparison that is evaluated in place (so nothing else needs its <c>&lt;N x i1&gt;</c> form) and
    /// its mask can be resized to the lane width of the selected values.
    /// </summary>
    private bool CanSelectOnComparisonMask(LLVMValueRef condition, LLVMValueRef selectedValue)
    {
        if (condition.Kind != LLVMValueKind.LLVMInstructionValueKind
            || condition.InstructionOpcode is not (LLVMOpcode.LLVMICmp or LLVMOpcode.LLVMFCmp)
            || !CanPushToStack(condition))
        {
            return false;
        }

        var comparedElementType = condition.GetOperand(0).TypeOf.ElementType;
        var selectedElementType = selectedValue.TypeOf.ElementType;

        // Pointer lanes are nint, which doesn't match the long vector of the mask.
        return comparedElementType.Kind != LLVMTypeKind.LLVMPointerTypeKind
            && selectedElementType.Kind != LLVMTypeKind.LLVMPointerTypeKind
            && TryResizeVectorMask(
                GetMaskLaneBits(comparedElementType),
                GetMaskLaneBits(selectedElementType),
                (int)selectedValue.TypeOf.VectorSize,
                emit: false);
    }

    private int GetMaskLaneBits(LLVMTypeRef elementType) =>
        TypeSystem.RoundUpToTypeSize(TypeSystem.GetSizeOfTypeInBits(elementType));

    /// <summary>
    /// Resizes the lanes of the full-width mask on the stack from <paramref name="fromLaneBits"/> to
    /// <paramref name="toLaneBits"/> bits. Every lane is all ones or all zeros, so truncating and sign-extending
    /// it loses nothing. Each step halves or doubles the lane width:
    /// <code>
    /// narrow: Vector128.Narrow(mask.GetLower(), mask.GetUpper())      // Vector256&lt;long&gt; to Vector128&lt;int&gt;
    /// widen:  Vector256.Create(Vector128.WidenLower(mask), Vector128.WidenUpper(mask))   // Vector128&lt;int&gt; to Vector256&lt;long&gt;
    /// </code>
    /// A mask of 16 or 32 bits (<c>&lt;2 x i8&gt;</c>, <c>&lt;2 x i16&gt;</c>, <c>&lt;4 x i8&gt;</c>) is in a
    /// <c>Vector64</c> with padding lanes, and a mask of 64 bits can be narrowed without splitting it, so those
    /// use only the lower half:
    /// <code>
    /// narrow: Vector64.Narrow(mask, mask)       // Vector64&lt;int&gt; to Vector64&lt;short&gt;
    /// widen:  Vector64.WidenLower(mask)         // Vector64&lt;sbyte&gt; to Vector64&lt;short&gt;
    /// </code>
    /// Returns whether it can be done, which needs the .NET vector types (64 to 512 bits) at every step: the
    /// custom <c>Vector1024</c> has neither <c>Narrow</c> nor <c>Widen</c>.
    /// With <paramref name="emit"/> false it only checks.
    /// </summary>
    private bool TryResizeVectorMask(int fromLaneBits, int toLaneBits, int laneCount, bool emit)
    {
        if (fromLaneBits == toLaneBits)
        {
            return true;
        }

        var laneBits = fromLaneBits;
        while (laneBits != toLaneBits)
        {
            var narrow = toLaneBits < laneBits;
            if (laneBits is not (8 or 16 or 32 or 64)
                || !CanEmitLaneWidthStep(laneCount, GetIntegerLaneType(laneBits, signed: true), narrow))
            {
                return false;
            }

            if (emit)
            {
                EmitLaneWidthStep(laneCount, GetIntegerLaneType(laneBits, signed: true), narrow);
            }

            laneBits = narrow ? laneBits / 2 : laneBits * 2;
        }

        return true;
    }

    /// <summary>
    /// Reinterprets the integer vector on the stack as a vector of <paramref name="elementType"/>,
    /// e.g. with <c>AsSingle</c>. Integer elements need nothing.
    /// </summary>
    private void EmitIntegerVectorAsElementType(LLVMTypeRef elementType, Type nonGenericVectorType)
    {
        var integerType = TypeSystem.GetIntegerType(TypeSystem.RoundUpToTypeSize(TypeSystem.GetSizeOfTypeInBits(elementType)));
        switch (elementType.Kind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsDouble)).MakeGenericMethod(integerType));
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Call, nonGenericVectorType.GetMethodStrict(nameof(Vector128.AsSingle)).MakeGenericMethod(integerType));
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                // Nothing to do.
                break;

            default:
                throw new NotImplementedException($"Unsupported operand element type: {elementType}");
        }
    }

    private readonly record struct SwitchCase(int ConstantValue, Label DestinationLabel)
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
            if (operands[i].Kind != LLVMValueKind.LLVMConstantIntValueKind
                || operands[i].TypeOf.IntWidth > 32)
            {
                throw new NotImplementedException();
            }

            // The condition is compared zero-extended, so the cases are too.
            var caseValue = (int)operands[i].ConstIntZExt;

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

            cases.Add(new SwitchCase(caseValue, destinationLabel));
        }
        cases.Sort();

        // Group the cases into clusters of sequential values.
        var clusters = new List<List<SwitchCase>>();
        foreach (var switchCase in cases)
        {
            if (clusters.Count > 0
                && clusters[^1] is var cluster
                && cluster[^1].ConstantValue + 1 == switchCase.ConstantValue)
            {
                cluster.Add(switchCase);
            }
            else
            {
                clusters.Add([switchCase]);
            }
        }

        // Each cluster tests the condition again. If it was inlined (so it's an expression, not a local),
        // evaluate it once into a local rather than once per cluster.
        LocalBuilder? conditionLocal = null;
        if (clusters.Count > 1 && CanPushToStack(condition))
        {
            // An int32 whatever the width: a narrower local would drop the zero-extension.
            conditionLocal = ILGenerator.DeclareLocal(typeof(int));
            EmitIntegerOperand(condition, Signedness.Unsigned);
            ILGenerator.Emit(OpCodes.Stloc, conditionLocal);
        }

        foreach (var cluster in clusters)
        {
            var caseValue = cluster[0].ConstantValue;

            if (conditionLocal != null)
            {
                ILGenerator.Emit(OpCodes.Ldloc, conditionLocal);
            }
            else
            {
                EmitIntegerOperand(condition, Signedness.Unsigned);
            }
            if (caseValue != 0)
            {
                ILGenerator.Emit(OpCodes.Ldc_I4, caseValue);
            }

            // If we have 2 or more cases, make a switch instruction.
            if (cluster.Count > 1)
            {
                if (caseValue != 0)
                {
                    ILGenerator.Emit(OpCodes.Sub);
                }
                ILGenerator.Emit(OpCodes.Switch, cluster.Select(x => x.DestinationLabel).ToArray());
            }
            else // Otherwise, do a normal conditional branch.
            {
                if (caseValue == 0)
                {
                    ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                }
                ILGenerator.Emit(OpCodes.Beq, cluster[0].DestinationLabel);
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

    private void EmitValueAddress(LLVMValueRef valueRef)
    {
        if (Locals.TryGetValue(valueRef, out var local))
        {
            ILGenerator.Emit(OpCodes.Ldloca, local);
        }
        else if (Parameters.TryGetValue(valueRef, out var parameter))
        {
            ILGenerator.Emit(OpCodes.Ldarga, parameter.Position - 1);
        }
        else
        {
            // A constant, or an instruction inlined into its user: it only exists on the stack, so spill
            // it to a temporary to have an address.
            var temporary = ILGenerator.DeclareLocal(TypeSystem.GetMsilType(valueRef.TypeOf));
            EmitValue(valueRef);
            ILGenerator.Emit(OpCodes.Stloc, temporary);
            ILGenerator.Emit(OpCodes.Ldloca, temporary);
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
                var containerBits = TypeSystem.RoundUpToTypeSize((int)typeRef.IntWidth);
                var loadBytes = ((int)typeRef.IntWidth + 7) / 8;
                if (containerBits <= 64 && loadBytes * 8 != containerBits)
                {
                    EmitLoadPartial(TypeSystem.GetMsilType(typeRef), loadBytes);
                    break;
                }

                switch (containerBits)
                {
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

                    case 128:
                        ILGenerator.Emit(OpCodes.Ldc_I4, ((int)typeRef.IntWidth + 7) / 8);
                        ILGenerator.EmitCall(OpCodes.Call,
                            typeof(WideIntegerHelper).GetStaticMethodStrict(nameof(WideIntegerHelper.LoadWideInt)),
                            null);
                        break;

                    default:
                        throw new NotImplementedException($"Int width {typeRef.IntWidth} not implemented: {typeRef}");
                }
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                ILGenerator.Emit(OpCodes.Ldind_I);
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
                // A vector held in a larger .NET vector (<2 x i8> in a Vector64<sbyte>, <3 x float> in a
                // Vector128<float>) has fewer bytes in memory than its container: reading the whole
                // container would go past the end of the object.
                if (TypeSystem.IsPaddedVector(typeRef))
                {
                    EmitLoadPartial(TypeSystem.GetMsilType(typeRef), TypeSystem.GetActualVectorSizeInBits(typeRef) / 8);
                    break;
                }

                goto case LLVMTypeKind.LLVMStructTypeKind;

            case LLVMTypeKind.LLVMArrayTypeKind:
            case LLVMTypeKind.LLVMStructTypeKind:
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