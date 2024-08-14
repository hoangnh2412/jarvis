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
}
