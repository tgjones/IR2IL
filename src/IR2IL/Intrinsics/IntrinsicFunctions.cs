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
        { "llvm.is.fpclass.f32", new LLVMIsFPClassIntrinsicFunction(isDouble: false) },
        { "llvm.is.fpclass.f64", new LLVMIsFPClassIntrinsicFunction(isDouble: true) },
        { "llvm.vector.reduce.add.v4i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector256), nameof(Vector256.Sum), typeof(long)) },
        { "llvm.vector.reduce.add.v8i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector256), nameof(Vector256.Sum), typeof(int)) },
        { "llvm.vector.reduce.add.v8i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector512), nameof(Vector512.Sum), typeof(long)) },
        { "llvm.vector.reduce.add.v2i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(long)) },
        { "llvm.vector.reduce.add.v4i16", StandardIntrinsicFunction.CreateGeneric(typeof(Vector64), nameof(Vector64.Sum), typeof(short)) },
        { "llvm.vector.reduce.add.v4i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(int)) },
        { "llvm.vector.reduce.add.v8i16", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(short)) },
        { "llvm.vector.reduce.add.v16i8", StandardIntrinsicFunction.CreateGeneric(typeof(Vector128), nameof(Vector128.Sum), typeof(sbyte)) },
        { "llvm.vector.reduce.add.v16i16", StandardIntrinsicFunction.CreateGeneric(typeof(Vector256), nameof(Vector256.Sum), typeof(short)) },
        { "llvm.vector.reduce.add.v12i32", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorReduceAddV12I32)) },
        { "llvm.vector.reduce.add.v16i32", StandardIntrinsicFunction.CreateGeneric(typeof(Vector512), nameof(Vector512.Sum), typeof(int)) },
        { "llvm.vector.reduce.add.v16i64", StandardIntrinsicFunction.CreateGeneric(typeof(Vector1024), nameof(Vector1024.Sum), typeof(long)) },

        // Standard intrinsics using custom methods.
        { "llvm.experimental.vector.partial.reduce.add.v4i32.v16i32", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.VectorPartialReduceAddV4I32V16I32)) },
        { "llvm.experimental.cttz.elts.i64.v2i1", StandardIntrinsicFunction.Create(typeof(LLVMIntrinsics), nameof(Runtime.LLVMIntrinsics.CttzElemsV2I1)) },

        // Irregular intrinsics.
        { "llvm.dbg.declare", new LLVMDbgDeclareIntrinsicFunction() },
        { "llvm.memcpy.p0.p0.i64", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memmove.p0.p0.i64", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memset.p0.i64", new LLVMMemSetI64IntrinsicFunction() },
        { "llvm.memcpy.p0.p0.i32", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memmove.p0.p0.i32", new LLVMMemCpyI64IntrinsicFunction() },
        { "llvm.memset.p0.i32", new LLVMMemSetI64IntrinsicFunction() },
        { "llvm.experimental.memset.pattern.p0.i16.i64", new LLVMMemSetPatternIntrinsicFunction(typeof(short)) },
        { "llvm.experimental.memset.pattern.p0.i32.i64", new LLVMMemSetPatternIntrinsicFunction(typeof(int)) },
        { "llvm.experimental.memset.pattern.p0.i64.i64", new LLVMMemSetPatternIntrinsicFunction(typeof(long)) },
        { "llvm.experimental.memset.pattern.p0.f32.i64", new LLVMMemSetPatternIntrinsicFunction(typeof(float)) },
        { "llvm.experimental.memset.pattern.p0.f64.i64", new LLVMMemSetPatternIntrinsicFunction(typeof(double)) },
        { "llvm.stacksave", new LLVMStackSaveIntrinsicFunction() },
        { "llvm.stacksave.p0", new LLVMStackSaveIntrinsicFunction() },
        { "llvm.stackrestore", new LLVMStackRestoreIntrinsicFunction() },
        { "llvm.stackrestore.p0", new LLVMStackRestoreIntrinsicFunction() },
        { "llvm.threadlocal.address.p0", new LLVMThreadLocalAddressIntrinsicFunction() },
        { "llvm.va_start.p0", new LLVMVaStartIntrinsicFunction() },

        { "llvm.expect.i1", new LLVMExpectIntrinsicFunction() },
        { "llvm.expect.i8", new LLVMExpectIntrinsicFunction() },
        { "llvm.expect.i16", new LLVMExpectIntrinsicFunction() },
        { "llvm.expect.i32", new LLVMExpectIntrinsicFunction() },
        { "llvm.expect.i64", new LLVMExpectIntrinsicFunction() },
        { "llvm.objectsize.i64.p0", new LLVMObjectSizeIntrinsicFunction(64) },
        { "llvm.objectsize.i32.p0", new LLVMObjectSizeIntrinsicFunction(32) },

        // No-op intrinsics.
        { "llvm.assume", NoOpIntrinsicFunction.Instance },
        { "llvm.dbg.label", NoOpIntrinsicFunction.Instance },
        { "llvm.dbg.value", NoOpIntrinsicFunction.Instance },
        { "llvm.experimental.noalias.scope.decl", NoOpIntrinsicFunction.Instance },
        { "llvm.lifetime.start.p0", NoOpIntrinsicFunction.Instance },
        { "llvm.lifetime.end.p0", NoOpIntrinsicFunction.Instance },
    };
}
