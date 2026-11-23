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
                if (valueTypeRef.IntWidth > 64)
                    EmitWideConstantIntegerValue(valueRef);
                else
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
                var (staticField, isExternalGlobal) = CompiledModule.GetGlobal(valueRef);
                // For internal globals, the static field IS the storage — push its address.
                // For external globals, the field holds the resolved native address — push that value.
                ILGenerator.Emit(isExternalGlobal ? OpCodes.Ldsfld : OpCodes.Ldsflda, staticField);
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

                            case 64:
                                ILGenerator.Emit(OpCodes.Ldc_I8, 0L);
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

    // ConstIntSExt only returns the lower 64 bits; for i128+ constants use PrintToString
    // which gives the full signed decimal, then parse as Int128.
    private void EmitWideConstantIntegerValue(LLVMValueRef valueRef)
    {
        var text = valueRef.PrintToString();  // e.g. "i128 36893488147419103232" or "i128 -1"
        var valueStr = text[(text.LastIndexOf(' ') + 1)..];
        EmitInt128(Int128.Parse(valueStr));
    }

    // Pushes an Int128 with the low `sizeInBits` bits set, to clear what is above bit N of a wide iN.
    protected void EmitWideIntegerMask(uint sizeInBits)
    {
        EmitInt128(sizeInBits >= 128 ? Int128.NegativeOne : (Int128)((UInt128.One << (int)sizeInBits) - 1));
    }

    private void EmitInt128(Int128 value)
    {
        ILGenerator.Emit(OpCodes.Ldc_I8, (long)((UInt128)value >> 64));
        ILGenerator.Emit(OpCodes.Ldc_I8, (long)(ulong)(UInt128)value);
        ILGenerator.Emit(OpCodes.Newobj, typeof(Int128).GetConstructorStrict([typeof(ulong), typeof(ulong)]));
    }

    // Pushes the value sign-extended into its container, like any other producer of an iN: the bits above
    // N are not defined, and the consumers extend the operand themselves (see FunctionILEmitter.EmitIntegerOperand).
    // Only for widths up to 64: wider constants are Int128, see EmitWideConstantIntegerValue.
    protected void EmitConstantIntegerValue(uint sizeInBits, long value)
    {
        switch (TypeSystem.RoundUpToTypeSize((int)sizeInBits))
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
                if (TypeSystem.IsPaddedVector(elementValue.TypeOf))
                {
                    // The field holds only the bytes of the vector.
                    ILGenerator.Emit(OpCodes.Ldflda, structFields[i]);
                    EmitConstantValue(elementValue, elementValue.TypeOf);
                    EmitStoreIndirect(elementValue.TypeOf);
                    continue;
                }

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
                ILGenerator.Emit(OpCodes.Stobj, TypeSystem.GetMsilType(type));
                break;

            case LLVMTypeKind.LLVMVectorTypeKind:
            {
                var msilVectorType = TypeSystem.GetMsilType(type);
                var actualBits = TypeSystem.GetActualVectorSizeInBits(type);
                if (actualBits != TypeSystem.GetVectorContainerSizeInBits(type))
                {
                    // The MSIL container type is larger than the true vector (e.g. <24 x float> = 768 bits
                    // stored in Vector1024<float> = 1024 bits, or <2 x i8> in a Vector64<sbyte>). Write only
                    // the actual bytes so we don't overflow into adjacent allocations.
                    EmitStorePartial(msilVectorType, actualBits / 8);
                }
                else
                {
                    ILGenerator.Emit(OpCodes.Stobj, msilVectorType);
                }
                break;
            }

            case LLVMTypeKind.LLVMDoubleTypeKind:
                ILGenerator.Emit(OpCodes.Stind_R8);
                break;

            case LLVMTypeKind.LLVMFloatTypeKind:
                ILGenerator.Emit(OpCodes.Stind_R4);
                break;

            case LLVMTypeKind.LLVMIntegerTypeKind:
                EmitStoreIntegerIndirect(type);
                break;

            case LLVMTypeKind.LLVMPointerTypeKind:
                ILGenerator.Emit(OpCodes.Stind_I);
                break;

            default:
                throw new NotImplementedException($"Unexpected type {type.Kind}: {type}");
        }
    }

    // An iN is stored in ceil(N/8) bytes (LLVM's store size), which for i24, i40, i72 and so on is less than
    // its MSIL container. Writing the whole container would clobber the bytes after it (a packed neighbour,
    // the next array element or the end of the allocation).
    // Stack on entry: [dest_ptr, value].
    private void EmitStoreIntegerIndirect(LLVMTypeRef type)
    {
        var width = (int)type.IntWidth;
        var containerBits = TypeSystem.RoundUpToTypeSize(width);
        var storeBytes = (width + 7) / 8;

        if (containerBits > 64 || storeBytes * 8 != containerBits)
        {
            var msilType = TypeSystem.GetMsilType(type);
            if (storeBytes * 8 == containerBits)
            {
                ILGenerator.Emit(OpCodes.Stobj, msilType);
            }
            else
            {
                EmitStorePartial(msilType, storeBytes);
            }
            return;
        }

        ILGenerator.Emit(containerBits switch
        {
            8 => OpCodes.Stind_I1,
            16 => OpCodes.Stind_I2,
            32 => OpCodes.Stind_I4,
            64 => OpCodes.Stind_I8,
            _ => throw new NotImplementedException($"Indirect store not implemented for integer width {width}: {type}")
        });
    }

    // Reads byteCount bytes (little-endian) into a zeroed value of msilType, for a value whose MSIL type is
    // larger than its LLVM type. Stack on entry: [ptr].
    protected void EmitLoadPartial(Type msilType, int byteCount)
    {
        var pointer = ILGenerator.DeclareLocal(typeof(nint));
        var value = ILGenerator.DeclareLocal(msilType);

        ILGenerator.Emit(OpCodes.Stloc, pointer);
        ILGenerator.Emit(OpCodes.Ldloca, value);
        ILGenerator.Emit(OpCodes.Initobj, msilType);
        ILGenerator.Emit(OpCodes.Ldloca, value);           // [dest]
        ILGenerator.Emit(OpCodes.Ldloc, pointer);          // [dest, src]
        ILGenerator.Emit(OpCodes.Ldc_I4, byteCount);       // [dest, src, count]
        ILGenerator.Emit(OpCodes.Conv_U);
        ILGenerator.Emit(OpCodes.Cpblk);
        ILGenerator.Emit(OpCodes.Ldloc, value);
    }

    // Writes the first byteCount bytes of a value of msilType (little-endian). Stack on entry: [dest_ptr, value].
    private void EmitStorePartial(Type msilType, int byteCount)
    {
        var tmp = ILGenerator.DeclareLocal(msilType);
        ILGenerator.Emit(OpCodes.Stloc, tmp);              // [dest_ptr]
        ILGenerator.Emit(OpCodes.Ldloca, tmp);             // [dest_ptr, &tmp]
        ILGenerator.Emit(OpCodes.Ldc_I4, byteCount);       // [dest_ptr, &tmp, count]
        ILGenerator.Emit(OpCodes.Conv_U);
        ILGenerator.Emit(OpCodes.Cpblk);
    }
}
