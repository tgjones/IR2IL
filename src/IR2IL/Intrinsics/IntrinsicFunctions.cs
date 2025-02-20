using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using IR2IL.Runtime;

namespace IR2IL.Intrinsics;

internal static class IntrinsicFunctions
{
    public static readonly Dictionary<string, IntrinsicFunction> LLVMIntrinsics = new()
    {
        // Standard intrinsics using Base Class Library methods.
        { "llvm.ceil.f64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Ceiling), typeof(double)) },
        { "llvm.copysign.f32", StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.CopySign)) },
        { "llvm.copysign.f64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.CopySign)) },
        { "llvm.ctpop.i8", StandardIntrinsicFunction.Create(typeof(sbyte), nameof(sbyte.PopCount), typeof(sbyte)) },
        { "llvm.fabs.f32", StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.Abs)) },
        { "llvm.fabs.f64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Abs), typeof(double)) },
        { "llvm.fmuladd.f32", StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.FusedMultiplyAdd)) },
        { "llvm.fmuladd.f64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.FusedMultiplyAdd)) },
        { "llvm.fmuladd.v2f32", StandardIntrinsicFunction.Create(typeof(Vector64), nameof(Vector64.FusedMultiplyAdd), typeof(Vector64<float>), typeof(Vector64<float>), typeof(Vector64<float>)) },
        { "llvm.fmuladd.v2f64", StandardIntrinsicFunction.Create(typeof(Vector128), nameof(Vector128.FusedMultiplyAdd), typeof(Vector128<double>), typeof(Vector128<double>), typeof(Vector128<double>)) },
        { "llvm.fmuladd.v4f32", StandardIntrinsicFunction.Create(typeof(Vector128), nameof(Vector128.FusedMultiplyAdd), typeof(Vector128<float>), typeof(Vector128<float>), typeof(Vector128<float>)) },
        { "llvm.fmuladd.v4f64", StandardIntrinsicFunction.Create(typeof(Vector256), nameof(Vector256.FusedMultiplyAdd), typeof(Vector256<double>), typeof(Vector256<double>), typeof(Vector256<double>)) },
        { "llvm.smin.i32", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Min), typeof(int), typeof(int)) },
        { "llvm.smax.i32", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Max), typeof(int), typeof(int)) },
        { "llvm.smax.i64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Max), typeof(long), typeof(long)) },
        { "llvm.smax.v4i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Max), typeof(int)) },
        { "llvm.sqrt.f32", StandardIntrinsicFunction.Create(typeof(MathF), nameof(MathF.Sqrt)) },
        { "llvm.sqrt.f64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Sqrt)) },
        { "llvm.umin.i32", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Min), typeof(uint), typeof(uint)) },
        { "llvm.umax.i64", StandardIntrinsicFunction.Create(typeof(Math), nameof(Math.Max), typeof(ulong), typeof(ulong)) },
        { "llvm.vector.reduce.add.v2i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(long)) },
        { "llvm.vector.reduce.add.v4i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(int)) },
        { "llvm.vector.reduce.add.v8i16", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(short)) },
        { "llvm.vector.reduce.add.v16i8", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(sbyte)) },
        { "llvm.vector.reduce.add.v16i16", StandardIntrinsicFunction.CreateGeneric(typeof(Vector256), nameof(Vector256.Sum), typeof(short)) },
        { "llvm.vector.reduce.add.v16i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector512), nameof(Vector512.Sum), typeof(int)) },
        { "llvm.vector.reduce.add.v16i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector1024), nameof(Vector1024.Sum), typeof(long)) },

        // Standard intrinsics using custom methods.
        { "llvm.vector.reduce.mul.v4i32", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorReduceMulV4I32)) },
        { "llvm.vector.reduce.mul.v16i8", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorReduceMulV16I8)) },
        { "llvm.vector.reduce.mul.v16i16", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorReduceMulV16I16)) },
        { "llvm.vector.reduce.smax.v4i32", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorReduceSMaxV4I32)) },

        // Irregular intrinsics.
        { "llvm.dbg.declare", new LLVMDbgDeclareIntrinsicFunction() },
        { "llvm.memcpy.p0.p0.i64", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memmove.p0.p0.i64", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memset.p0.i64", new LLVMMemSetI64IntrinsicFunction() },
        { "llvm.stacksave", new LLVMStackSaveIntrinsicFunction() },
        { "llvm.stacksave.p0", new LLVMStackSaveIntrinsicFunction() },
        { "llvm.threadlocal.address.p0", new LLVMThreadLocalAddressIntrinsicFunction() },
        { "llvm.usub.sat.i32", new LLVMUSubSatI32IntrinsicFunction() },
        { "llvm.va_start", new LLVMVaStartIntrinsicFunction() },

        // No-op intrinsics.
        { "llvm.assume", NoOpIntrinsicFunction.Instance },
        { "llvm.dbg.label", NoOpIntrinsicFunction.Instance },
        { "llvm.dbg.value", NoOpIntrinsicFunction.Instance },
        { "llvm.experimental.noalias.scope.decl", NoOpIntrinsicFunction.Instance },
        { "llvm.lifetime.start.p0", NoOpIntrinsicFunction.Instance },
        { "llvm.lifetime.end.p0", NoOpIntrinsicFunction.Instance },
        { "llvm.stackrestore", NoOpIntrinsicFunction.Instance },
        { "llvm.stackrestore.p0", NoOpIntrinsicFunction.Instance }
    };
}
