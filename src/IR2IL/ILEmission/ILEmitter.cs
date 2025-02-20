using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using IR2IL.Helpers;
using LLVMSharp.Interop;

namespace IR2IL.ILEmission;

internal abstract class ILEmitter
{
    protected CompiledModule CompiledModule { get; }
    protected TypeSystem TypeSystem { get; }
    protected ILGenerator ILGenerator { get; }

    public ILEmitter(CompiledModule compiledModule, ILGenerator ilGenerator)
    {
        CompiledModule = compiledModule;
        TypeSystem = compiledModule.TypeSystem;
        ILGenerator = ilGenerator;
    }

    protected void EmitConstantValue(LLVMValueRef valueRef, LLVMTypeRef valueTypeRef)
    {
        switch (valueRef.Kind)
        {
            case LLVMValueKind.LLVMConstantAggregateZeroValueKind:
                switch (valueTypeRef.Kind)
                {
                    case LLVMTypeKind.LLVMArrayTypeKind:
                        EmitLoadConstantArray(valueRef, valueTypeRef);
                        break;

                    case LLVMTypeKind.LLVMStructTypeKind:
                        EmitLoadConstantStruct(valueRef, TypeSystem.GetMsilType(valueTypeRef));
                        break;

                    case LLVMTypeKind.LLVMVectorTypeKind:
                        ILGenerator.Emit(OpCodes.Call, TypeSystem.GetMsilVectorType(valueTypeRef).GetMethodStrict("get_Zero"));
                        break;

                    default:
                        throw new NotImplementedException($"Constant aggregate zero value {valueTypeRef.Kind} not implemented: {valueRef}");
                }
                break;

            case LLVMValueKind.LLVMConstantDataArrayValueKind:
            case LLVMValueKind.LLVMConstantArrayValueKind:
                EmitLoadConstantArray(valueRef, valueTypeRef);
                break;

            case LLVMValueKind.LLVMConstantDataVectorValueKind:
            case LLVMValueKind.LLVMConstantVectorValueKind:
                EmitLoadConstantVector(valueRef, valueTypeRef);
                break;

            case LLVMValueKind.LLVMConstantFPValueKind:
                var constRealDouble = valueRef.GetConstRealDouble(out var losesInfo);
                if (losesInfo)
                {
                    throw new InvalidOperationException();
                }
                switch (valueTypeRef.Kind)
                {
                    case LLVMTypeKind.LLVMDoubleTypeKind:
                        ILGenerator.Emit(OpCodes.Ldc_R8, constRealDouble);
                        break;

                    case LLVMTypeKind.LLVMFloatTypeKind:
                        ILGenerator.Emit(OpCodes.Ldc_R4, (float)constRealDouble);
                        break;

                    default:
                        throw new NotImplementedException($"Unsuported constant floating-point type: {valueTypeRef}");
                }
                break;

            case LLVMValueKind.LLVMConstantIntValueKind:
                EmitConstantIntegerValue(valueTypeRef.IntWidth, valueRef.ConstIntSExt);
                break;

            case LLVMValueKind.LLVMConstantExprValueKind:
                switch (valueRef.ConstOpcode)
                {
                    case LLVMOpcode.LLVMGetElementPtr:
                        EmitConstantValue(valueRef.GetOperand(0), valueRef.GetOperand(0).TypeOf);
                        ILGenerator.Emit(OpCodes.Ldc_I4, GetElementPtrConst(valueRef));
                        ILGenerator.Emit(OpCodes.Conv_U);
                        ILGenerator.Emit(OpCodes.Add);
                        break;

                    case LLVMOpcode.LLVMIntToPtr:
                        EmitConstantValue(valueRef.GetOperand(0), valueRef.GetOperand(0).TypeOf);
                        ILGenerator.Emit(OpCodes.Conv_U);
                        break;

                    case LLVMOpcode.LLVMPtrToInt:
                        EmitConstantValue(valueRef.GetOperand(0), valueRef.GetOperand(0).TypeOf);
                        EmitConvertToInt(valueRef.TypeOf.IntWidth);
                        break;

                    default:
                        throw new NotImplementedException($"Const opcode {valueRef.ConstOpcode} not implemented: {valueRef}");
                }
                break;

            case LLVMValueKind.LLVMConstantPointerNullValueKind:
                ILGenerator.Emit(OpCodes.Ldc_I4_0);
                ILGenerator.Emit(OpCodes.Conv_U);
                break;

            case LLVMValueKind.LLVMConstantStructValueKind:
                EmitLoadConstantStruct(valueRef, TypeSystem.GetMsilType(valueTypeRef));
                break;

            case LLVMValueKind.LLVMFunctionValueKind:
                ILGenerator.Emit(OpCodes.Ldftn, CompiledModule.GetFunction(valueRef));
                break;

            case LLVMValueKind.LLVMGlobalVariableValueKind:
                var staticField = CompiledModule.GetGlobal(valueRef);
                ILGenerator.Emit(OpCodes.Ldsflda, staticField);
                break;

            case LLVMValueKind.LLVMPoisonValueValueKind:
            case LLVMValueKind.LLVMUndefValueValueKind:
                switch (valueTypeRef.Kind)
                {
                    case LLVMTypeKind.LLVMArrayTypeKind:
                        EmitLoadConstantArray(valueRef, valueTypeRef);
                        break;

                    case LLVMTypeKind.LLVMFloatTypeKind:
                        ILGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
                        break;

                    case LLVMTypeKind.LLVMIntegerTypeKind:
                        switch (TypeSystem.RoundUpToTypeSize((int)valueTypeRef.IntWidth))
                        {
                            case 8:
                            case 16:
                            case 32:
                                ILGenerator.Emit(OpCodes.Ldc_I4_0);
                                break;

                            default:
                                throw new NotImplementedException($"Emit constant not implemented for {valueRef}");
                        }
                        break;

                    case LLVMTypeKind.LLVMPointerTypeKind:
                        ILGenerator.Emit(OpCodes.Ldc_I4_0);
                        ILGenerator.Emit(OpCodes.Conv_U);
                        break;

                    case LLVMTypeKind.LLVMStructTypeKind:
                        EmitLoadConstantStruct(valueRef, TypeSystem.GetMsilType(valueTypeRef));
                        break;

                    case LLVMTypeKind.LLVMVectorTypeKind:
                        ILGenerator.Emit(OpCodes.Call, TypeSystem.GetMsilVectorType(valueTypeRef).GetMethodStrict("get_Zero"));
                        break;

                    default:
                        throw new NotImplementedException($"Unsupported poison / undef value type kind {valueTypeRef.Kind}: {valueRef}");
                }
                break;

            default:
                throw new NotImplementedException($"Unsupported value kind {valueRef.Kind}: {valueRef}");
        }
    }

    protected void EmitConvertToInt(uint sizeInBits)
    {
        switch (TypeSystem.RoundUpToTypeSize((int)sizeInBits))
        {
            case 32:
                ILGenerator.Emit(OpCodes.Conv_I4);
                break;

            case 64:
                ILGenerator.Emit(OpCodes.Conv_I8);
                break;

            default:
                throw new NotImplementedException($"Convert to integer width {sizeInBits} not implemented");
        }
    }

    protected void EmitConstantIntegerValue(uint sizeInBits, long value)
    {
        var roundedUpBits = TypeSystem.RoundUpToTypeSize((int)sizeInBits);

        if (roundedUpBits > sizeInBits)
        {
            var mask = (1 << (int)sizeInBits) - 1;
            if (value >= 0)
            {
                value = value & mask;
            }
            else if (roundedUpBits == 8)
            {
                value = (byte)(sbyte)value & mask;
            }
            else
            {

            }
        }

        switch (roundedUpBits)
        {
            case 8:
            case 16:
            case 32:
                ILGenerator.Emit(OpCodes.Ldc_I4, (int)value);
                break;

            case 64:
                ILGenerator.Emit(OpCodes.Ldc_I8, value);
                break;

            default:
                throw new NotImplementedException($"Load constant integer width {sizeInBits} not implemented");
        }
    }

    private unsafe int GetElementPtrConst(LLVMValueRef constExpr)
    {
        var sourceElementType = (LLVMTypeRef)LLVM.GetGEPSourceElementType(constExpr);
        var currentType = sourceElementType;

        var result = 0;

        if (constExpr.GetOperand(1).Kind != LLVMValueKind.LLVMConstantIntValueKind)
        {
            throw new NotImplementedException();
        }

        var sizeInBytes = (long)TypeSystem.GetSizeOfTypeInBytes(currentType);
        result += (int)(constExpr.GetOperand(1).ConstIntSExt * sizeInBytes);

        for (var i = 2u; i < constExpr.OperandCount; i++)
        {
            var index = constExpr.GetOperand(i);

            if (index.Kind != LLVMValueKind.LLVMConstantIntValueKind)
            {
                throw new NotImplementedException();
            }

            switch (currentType.Kind)
            {
                case LLVMTypeKind.LLVMArrayTypeKind:
                    result += (int)index.ConstIntSExt * TypeSystem.GetSizeOfTypeInBytes(currentType.ElementType);
                    currentType = currentType.ElementType;
                    break;

                case LLVMTypeKind.LLVMIntegerTypeKind:
                    result += (int)index.ConstIntSExt;
                    break;

                case LLVMTypeKind.LLVMStructTypeKind:
                    var fieldIndex = (uint)index.ConstIntSExt;
                    result += TypeSystem.GetStructFieldOffset(currentType, fieldIndex);
                    currentType = currentType.StructGetTypeAtIndex(fieldIndex);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        return result;
    }

    private void EmitLoadConstantVector(LLVMValueRef vectorValue, LLVMTypeRef vectorType)
    {
        var vectorSizeInBytes = TypeSystem.GetSizeOfTypeInBytes(vectorType);

        switch (vectorSizeInBytes)
        {
            case 8:
            case 16:
            case 32:
            case 64:
                var elementType = TypeSystem.GetMsilType(vectorType.ElementType);
                for (var i = 0u; i < vectorType.VectorSize; i++)
                {
                    EmitConstantValue(vectorValue.GetAggregateElement(i), vectorType.ElementType);
                }
                var createMethodParameterTypes = new Type[vectorType.VectorSize];
                Array.Fill(createMethodParameterTypes, elementType);
                ILGenerator.Emit(OpCodes.Call, TypeSystem.GetNonGenericVectorType(vectorType).GetMethodStrict(nameof(Vector128.Create), createMethodParameterTypes));
                break;

            default:
                EmitLoadConstantArrayOrVector(vectorValue, vectorType, (int)vectorType.VectorSize);
                break;
        }
    }

    private void EmitLoadConstantArray(LLVMValueRef arrayValue, LLVMTypeRef arrayValueType)
    {
        EmitLoadConstantArrayOrVector(
            arrayValue,
            arrayValueType,
            (int)arrayValueType.ArrayLength);
    }

    private void EmitLoadConstantArrayOrVector(
        LLVMValueRef arrayOrVectorValue,
        LLVMTypeRef arrayOrVectorValueType,
        int length)
    {
        var msilType = TypeSystem.GetMsilType(arrayOrVectorValueType);

        var local = ILGenerator.DeclareLocal(msilType);

        ILGenerator.Emit(OpCodes.Ldloca, local);
        ILGenerator.Emit(OpCodes.Initobj, msilType);

        switch (arrayOrVectorValue.Kind)
        {
            case LLVMValueKind.LLVMConstantArrayValueKind:
            case LLVMValueKind.LLVMConstantDataArrayValueKind:
            case LLVMValueKind.LLVMConstantDataVectorValueKind:
            case LLVMValueKind.LLVMConstantVectorValueKind:
                var elementSizeInBytes = Math.Max(TypeSystem.GetSizeOfTypeInBits(arrayOrVectorValueType.ElementType), 8) / 8;
                for (var i = 0; i < length; i++)
                {
                    ILGenerator.Emit(OpCodes.Ldloca, local);
                    if (i > 0)
                    {
                        ILGenerator.Emit(OpCodes.Ldc_I4, i * elementSizeInBytes);
                        ILGenerator.Emit(OpCodes.Conv_U);
                        ILGenerator.Emit(OpCodes.Add);
                    }
                    var elementValue = arrayOrVectorValue.GetAggregateElement((uint)i);
                    // TODO: This is rather hacky. But we need to handle the situation where
                    // i1 constant values in a vector should be emitted as i8, whereas normal
                    // scalar values not in a vector should be emitted as i1.
                    if (arrayOrVectorValueType.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                        && arrayOrVectorValueType.ElementType.IntWidth == 1)
                    {
                        ILGenerator.Emit(OpCodes.Ldc_I4, (int)elementValue.ConstIntSExt);
                    }
                    else
                    {
                        EmitConstantValue(elementValue, arrayOrVectorValueType.ElementType);
                    }
                    EmitStoreIndirect(arrayOrVectorValueType.ElementType);
                }
                break;
        }

        ILGenerator.Emit(OpCodes.Ldloc, local);
    }

    private void EmitLoadConstantStruct(LLVMValueRef structValue, Type structType)
    {
        var local = ILGenerator.DeclareLocal(structType);

        ILGenerator.Emit(OpCodes.Ldloca, local);
        ILGenerator.Emit(OpCodes.Initobj, structType);

        if (structValue.Kind == LLVMValueKind.LLVMConstantStructValueKind)
        {
            var structFields = structType.GetFields();
            for (var i = 0; i < structFields.Length; i++)
            {
                ILGenerator.Emit(OpCodes.Ldloca, local);
                var elementValue = structValue.GetAggregateElement((uint)i);
                EmitConstantValue(elementValue, elementValue.TypeOf);
                ILGenerator.Emit(OpCodes.Stfld, structFields[i]);
            }
        }

        ILGenerator.Emit(OpCodes.Ldloc, local);
    }

    protected void EmitStoreIndirect(LLVMTypeRef type)
    {
        switch (type.Kind)
        {
            case LLVMTypeKind.LLVMArrayTypeKind:
            case LLVMTypeKind.LLVMStructTypeKind:
            case LLVMTypeKind.LLVMVectorTypeKind:
                ILGenerator.Emit(OpCodes.Stobj, TypeSystem.GetMsilType(type));
                break;

            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Stind_R8);
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Stind_R4);
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                ILGenerator.Emit(type.IntWidth switch
                {
                    1 => OpCodes.Stind_I1,
                    8 => OpCodes.Stind_I1,
                    16 => OpCodes.Stind_I2,
                    32 => OpCodes.Stind_I4,
                    64 => OpCodes.Stind_I8,
                    _ => throw new NotImplementedException($"Indirect store not implemented for integer width {type.IntWidth}: {type}")
                });
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                ILGenerator.Emit(OpCodes.Stind_I);
                break;

            default:
                throw new NotImplementedException($"Unexpected type {type.Kind}: {type}");
        }
    }
}
