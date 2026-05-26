using System.Reflection;

namespace Jarvis.Domain.Shared.Utility;

public static class TypeHelper
{
    /// <summary>
    /// Get all subtypes of a type that is a class
    /// </summary>
    /// <typeparam name="TBaseType">The base type</typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllClassSubTypes<TBaseType>()
    {
        var type = typeof(TBaseType);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
        return types;
    }

    public static IEnumerable<Type> GetConstructedGenericInterfaceWithTypeArgumentsTypes(Type genericInterfaceType)
    {
        if (!genericInterfaceType.IsGenericType)
            throw new ArgumentException($"{nameof(genericInterfaceType)} must be a generic type definition.");

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var constructedTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == genericInterfaceType && type.GenericTypeArguments.Length > 0)
                    {
                        constructedTypes.Add(type);
                    }
                    else
                    {
                        var interfaces = type.GetInterfaces();
                        foreach (var interfaceType in interfaces)
                        {
                            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType && interfaceType.GenericTypeArguments.Length > 0)
                            {
                                constructedTypes.Add(interfaceType);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Ignore any assemblies that fail to load
            }
        }

        return constructedTypes;
    }

    public static List<Type> GetInterfacesOfGenericType(Type genericType)
    {
        if (!genericType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The provided type must be a generic type definition.", nameof(genericType));
        }

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(SafeGetTypes) // Get all types safely from loaded assemblies
            .Where(type => type.IsInterface && type.IsGenericType) // Only interfaces with generic types
            .Where(type => type.GetGenericTypeDefinition() == genericType ||
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)).ToList();
    }

    public static IEnumerable<Type> GetConstructedGenericInterfaces(Type genericInterfaceType)
    {
        if (!genericInterfaceType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The provided type must be a generic type definition.", nameof(genericInterfaceType));
        }

        return AppDomain.CurrentDomain
            .GetAssemblies() // Load all assemblies in the app domain
            .SelectMany(SafeGetTypes) // Get types safely
            .Where(type => type.IsInterface && type.IsGenericType) // Only generic interfaces
            .Where(type => type.GetGenericTypeDefinition() == genericInterfaceType); // Match generic type definition
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    public static bool IsNumericType(Type type)
    {
        if (type == null) return false;

        // First, handle nullable types by getting the underlying type
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Use TypeCode to determine numeric types
        switch (Type.GetTypeCode(underlyingType))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}
