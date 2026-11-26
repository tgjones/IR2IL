using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IR2IL.Helpers;

namespace IR2IL.Intrinsics;

/// <summary>
/// Helpers shared by the intrinsic resolvers that call the .NET vector APIs (<c>Vector64</c> … <c>Vector512</c>
/// and <c>Vector1024</c>) directly.
/// </summary>
internal static class VectorIntrinsicEmitter
{
    /// <summary>
    /// The BCL (or <c>Vector1024</c>) method <paramref name="name"/> of <paramref name="nonGenericVectorType"/> that takes
    /// <paramref name="arity"/> vectors of type <paramref name="vectorType"/>: a concrete overload
    /// (<c>Vector128.Sin(Vector128&lt;float&gt;)</c>) if there is one, else the generic definition
    /// (<c>Vector128.Max&lt;T&gt;(Vector128&lt;T&gt;, Vector128&lt;T&gt;)</c>) made for <paramref name="laneType"/>.
    /// </summary>
    public static MethodInfo GetVectorMethod(Type nonGenericVectorType, string name, Type vectorType, Type laneType, int arity)
    {
        var concrete = nonGenericVectorType.GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Static,
            Enumerable.Repeat(vectorType, arity).ToArray());
        if (concrete != null)
        {
            return concrete;
        }

        var definition = vectorType.GetGenericTypeDefinition();
        var generic = nonGenericVectorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == name
                && m.IsGenericMethodDefinition
                && m.GetGenericArguments().Length == 1
                && m.GetParameters().Length == arity
                && m.GetParameters().All(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == definition));
        return generic?.MakeGenericMethod(laneType)
            ?? throw new InvalidOperationException($"{nonGenericVectorType.Name}.{name} not found for {vectorType}.");
    }

    /// <summary>Emits <c>N.As&lt;TFrom, TTo&gt;</c> on the vector at the top of the stack.</summary>
    public static void EmitReinterpret(ILGenerator il, Type nonGenericVectorType, Type fromLane, Type toLane)
    {
        if (fromLane == toLane)
        {
            return;
        }

        var method = nonGenericVectorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "As" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2)
            .MakeGenericMethod(fromLane, toLane);
        il.Emit(OpCodes.Call, method);
    }

    /// <summary>Emits a constant of <paramref name="laneType"/>.</summary>
    public static void EmitLaneConstant(ILGenerator il, Type laneType, object value)
    {
        switch (Type.GetTypeCode(laneType))
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
                // Narrow constants are pushed as an int; the truncation to the lane width is the caller's value.
                il.Emit(OpCodes.Ldc_I4, unchecked((int)Convert.ToInt64(value)));
                break;
            case TypeCode.Int64:
                il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
                break;
            case TypeCode.UInt64:
                il.Emit(OpCodes.Ldc_I8, unchecked((long)Convert.ToUInt64(value)));
                break;
            case TypeCode.Single:
                il.Emit(OpCodes.Ldc_R4, Convert.ToSingle(value));
                break;
            case TypeCode.Double:
                il.Emit(OpCodes.Ldc_R8, Convert.ToDouble(value));
                break;
            default:
                throw new NotImplementedException($"Vector lanes of type {laneType.Name}.");
        }
    }

    /// <summary>
    /// Replaces the padding lanes of the vector at the top of the stack (lanes <paramref name="laneCount"/> and above) with
    /// <paramref name="identity"/>: <c>ConditionalSelect(LessThan(Indices, Create(laneCount)), v, Create(identity))</c>.
    /// </summary>
    public static void EmitFillPaddingLanes(ILGenerator il, Type nonGenericVectorType, Type vectorType, Type laneType, int laneCount, object identity)
    {
        if (nonGenericVectorType.Name == "Vector1024")
        {
            throw new NotImplementedException("Padded Vector1024 vectors.");
        }

        var vector = il.DeclareLocal(vectorType);
        il.Emit(OpCodes.Stloc, vector);

        var create = nonGenericVectorType.GetMethodStrict("Create", [laneType]);

        // The mask: Indices < Create(laneCount). Indices of floating point lanes are floating point too.
        il.Emit(OpCodes.Call, vectorType.GetPropertyStrict("Indices").GetMethod!);
        EmitLaneConstant(il, laneType, laneCount);
        il.Emit(OpCodes.Call, create);
        il.Emit(OpCodes.Call, GetVectorMethod(nonGenericVectorType, "LessThan", vectorType, laneType, 2));

        il.Emit(OpCodes.Ldloc, vector);
        EmitLaneConstant(il, laneType, identity);
        il.Emit(OpCodes.Call, create);
        il.Emit(OpCodes.Call, GetVectorMethod(nonGenericVectorType, "ConditionalSelect", vectorType, laneType, 3));
    }

    /// <summary>
    /// The scalar fallback: stores the <paramref name="arity"/> vector operands at the top of the stack in locals, then
    /// for each real lane emits <c>GetElement</c> of every operand, a call to <paramref name="scalar"/> and
    /// <c>WithElement</c>, starting from the first operand, so the padding lanes keep its values.
    /// </summary>
    public static void EmitPerLane(ILGenerator il, Type nonGenericVectorType, Type vectorType, Type laneType, int laneCount, int arity, MethodInfo scalar)
    {
        var getElement = nonGenericVectorType.GetStaticMethodStrict(nameof(System.Runtime.Intrinsics.Vector128.GetElement)).MakeGenericMethod(laneType);
        var withElement = nonGenericVectorType.GetStaticMethodStrict(nameof(System.Runtime.Intrinsics.Vector128.WithElement)).MakeGenericMethod(laneType);

        var operands = new LocalBuilder[arity];
        for (var k = arity - 1; k >= 0; k--)
        {
            operands[k] = il.DeclareLocal(vectorType);
            il.Emit(OpCodes.Stloc, operands[k]);
        }

        il.Emit(OpCodes.Ldloc, operands[0]);
        for (var lane = 0; lane < laneCount; lane++)
        {
            il.Emit(OpCodes.Ldc_I4, lane);
            foreach (var operand in operands)
            {
                il.Emit(OpCodes.Ldloc, operand);
                il.Emit(OpCodes.Ldc_I4, lane);
                il.Emit(OpCodes.Call, getElement);
            }

            il.Emit(OpCodes.Call, scalar);
            il.Emit(OpCodes.Call, withElement);
        }
    }
}
