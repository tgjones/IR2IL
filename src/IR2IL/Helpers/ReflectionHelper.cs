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

    public static MethodInfo GetStaticMethodStrict(this Type type, string methodName)
    {
        return type.GetMethodStrict(methodName, BindingFlags.Public | BindingFlags.Static);
    }
}
