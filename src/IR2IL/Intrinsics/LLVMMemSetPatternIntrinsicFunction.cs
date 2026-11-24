using System;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;
using IR2IL.Runtime;

namespace IR2IL.Intrinsics;

/// <summary>
/// <c>declare void @llvm.experimental.memset.pattern.p0.&lt;type&gt;.i64(ptr &lt;dest&gt;, &lt;type&gt; &lt;pattern&gt;, i64 &lt;count&gt;, i1 &lt;isvolatile&gt;)</c>:
/// fills <c>count</c> elements of the pattern's type. The pattern type is the .NET type of the lanes it is stored as.
/// </summary>
internal sealed class LLVMMemSetPatternIntrinsicFunction(Type patternType) : IntrinsicFunction
{
    private readonly MethodInfo _method = typeof(LLVMIntrinsics)
        .GetStaticMethodStrict(nameof(LLVMIntrinsics.MemSetPattern))
        .MakeGenericMethod(patternType);

    public override void BuildCall(IntrinsicFunctionCallContext context)
    {
        context.EmitValue(context.Operands[0]);
        context.EmitValue(context.Operands[1]);
        context.EmitValue(context.Operands[2]);
        context.ILGenerator.Emit(OpCodes.Call, _method);

        // TODO: isVolatile
    }
}
