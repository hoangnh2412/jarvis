using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Platform.Modules.ImportExport.Application.Schemas.Models;

namespace Platform.Modules.ImportExport.Application.Schemas;

internal static class ReflectionFieldReader
{
    private static readonly HashSet<string> ExcludedNames =
        new(StringComparer.OrdinalIgnoreCase) { "Id", "TenantId" };

    private static readonly HashSet<string> NumericDataTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "int", "long", "decimal", "double", "float", "number"
        };

    public static IReadOnlyList<ImportExportFieldSchema> Read(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Where(p => !ExcludedNames.Contains(p.Name))
            .Where(p => IsSupportedScalar(p.PropertyType))
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(MapProperty)
            .ToList();
    }

    private static bool IsSupportedScalar(Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return underlying.IsEnum
               || underlying == typeof(string)
               || underlying == typeof(bool)
               || underlying == typeof(int)
               || underlying == typeof(long)
               || underlying == typeof(decimal)
               || underlying == typeof(double)
               || underlying == typeof(float)
               || underlying == typeof(DateTime)
               || underlying == typeof(DateTimeOffset)
               || underlying == typeof(Guid);
    }

    private static ImportExportFieldSchema MapProperty(PropertyInfo property)
    {
        var required = property.GetCustomAttribute<RequiredAttribute>() is not null;
        var minLength = property.GetCustomAttribute<MinLengthAttribute>()?.Length;
        var maxLength = property.GetCustomAttribute<MaxLengthAttribute>()?.Length;
        var range = property.GetCustomAttribute<RangeAttribute>();
        decimal? min = range is not null && TryParseDecimal(range.Minimum, out var parsedMin)
            ? parsedMin
            : null;
        decimal? max = range is not null && TryParseDecimal(range.Maximum, out var parsedMax)
            ? parsedMax
            : null;
        var regex = property.GetCustomAttribute<RegularExpressionAttribute>()?.Pattern;
        var dataType = MapDataType(property.PropertyType);

        return new ImportExportFieldSchema(
            property.Name,
            property.Name,
            dataType,
            property.PropertyType.FullName ?? property.PropertyType.Name,
            required,
            minLength,
            maxLength,
            min,
            max,
            regex,
            NumericDataTypes.Contains(dataType));
    }

    private static bool TryParseDecimal(object? value, out decimal result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        return decimal.TryParse(value.ToString(), out result);
    }

    private static string MapDataType(Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (underlying == typeof(string))
            return "string";
        if (underlying.IsEnum)
            return "enum";
        if (underlying == typeof(bool))
            return "boolean";
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
            return "datetime";
        if (underlying == typeof(Guid))
            return "guid";
        if (underlying == typeof(int))
            return "int";
        if (underlying == typeof(long))
            return "long";
        if (underlying == typeof(decimal))
            return "decimal";
        if (underlying == typeof(double))
            return "double";
        if (underlying == typeof(float))
            return "float";
        return "number";
    }
}
