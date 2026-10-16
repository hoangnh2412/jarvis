using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Platform.ImportExport.MiniExcel.Reflection;

internal static class PropertyAccessor
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> Cache = new();

    public static object? GetValue(object? source, string propertyName)
    {
        if (source is null)
            return null;

        if (TryGetFromDictionary(source, propertyName, out var dictionaryValue))
            return dictionaryValue;

        var properties = Cache.GetOrAdd(source.GetType(), static type =>
        {
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                    map[property.Name] = property;
            }

            return map;
        });

        return properties.TryGetValue(propertyName, out var property)
            ? property.GetValue(source)
            : null;
    }

    private static bool TryGetFromDictionary(object source, string propertyName, out object? value)
    {
        switch (source)
        {
            case IReadOnlyDictionary<string, object?> readOnlyNullable when
                readOnlyNullable.TryGetValue(propertyName, out value):
                return true;

            case IDictionary<string, object?> nullable when
                nullable.TryGetValue(propertyName, out value):
                return true;

            case IDictionary<string, object> nonNullable when
                nonNullable.TryGetValue(propertyName, out var nonNullValue):
                value = nonNullValue;
                return true;

            case IDictionary dictionary:
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is string key &&
                        string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = entry.Value;
                        return true;
                    }
                }

                break;
        }

        value = null;
        return false;
    }
}
