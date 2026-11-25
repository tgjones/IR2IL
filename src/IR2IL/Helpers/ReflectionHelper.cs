using System;
using System.Linq;
using System.Reflection;

namespace IR2IL.Helpers;

internal static class ReflectionHelper
{
    public static ConstructorInfo GetConstructorStrict(this Type type, Type[] types)
    {
        return type.GetConstructor(types) ?? throw new InvalidOperationException($"Constructor with parameters {string.Join(", ", types.Select(x => x.ToString()))} not found in type {type}.");
    }

    public static PropertyInfo GetPropertyStrict(this Type type, string propertyName)
    {
        return type.GetProperty(propertyName) ?? throw new InvalidOperationException($"Property {propertyName} not found in type {type}.");
    }

    public static MethodInfo GetMethodStrict(this Type type, string methodName, Type[] types)
    {
        return type.GetMethod(methodName, types) ?? throw new InvalidOperationException($"Method {methodName} with parameters {string.Join(", ", types.Select(x => x.ToString()))} not found in type {type}.");
    }

    public static MethodInfo GetMethodStrict(this Type type, string methodName, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
    {
        return type.GetMethod(methodName, bindingFlags) ?? throw new InvalidOperationException($"Method {methodName} not found in type {type}.");
    }

    /// <summary>
    /// The generic method definition called <paramref name="methodName"/>, for a name that is also used by non-generic
    /// overloads (where <see cref="GetStaticMethodStrict"/> would find several methods).
    /// </summary>
    public static MethodInfo GetGenericMethodDefinitionStrict(this Type type, string methodName)
    {
        var definitions = type
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name == methodName && x.IsGenericMethodDefinition)
            .ToArray();

        return definitions.Length == 1
            ? definitions[0]
            : throw new InvalidOperationException($"Expected one generic method {methodName} in type {type}, found {definitions.Length}.");
    }

    public static MethodInfo GetStaticMethodStrict(this Type type, string methodName)
    {
        return type.GetMethodStrict(methodName, BindingFlags.Public | BindingFlags.Static);
    }
}
