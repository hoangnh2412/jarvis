using System.Text.Json;
using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Exceptions;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>
/// Parses Mappings JSON for both API host and DLL consumers.
/// Rejects unknown properties (e.g. columnNameAAAA) before domain validation.
/// </summary>
public static class ColumnMappingJsonParser
{
    private static readonly HashSet<string> KnownProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        MappingJsonConstants.Properties.ColumnName,
        MappingJsonConstants.Properties.FieldName,
        MappingJsonConstants.Properties.Required,
        MappingJsonConstants.Properties.Unique,
        MappingJsonConstants.Properties.Regex,
        MappingJsonConstants.Properties.MinLength,
        MappingJsonConstants.Properties.MaxLength,
        MappingJsonConstants.Properties.Min,
        MappingJsonConstants.Properties.Max
    };

    public static List<ColumnMapping> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return ParseParts([json]);
    }

    public static List<ColumnMapping> Parse(IEnumerable<string?> values) => ParseParts(values);

    private static List<ColumnMapping> ParseParts(IEnumerable<string?> values)
    {
        var parts = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => NormalizeJson(v!))
            .Where(v => !IsAutoMapToken(v))
            .ToList();

        if (parts.Count == 0)
            return [];

        try
        {
            var mappings = new List<ColumnMapping>();

            foreach (var part in parts)
            {
                using var document = JsonDocument.Parse(part);
                var root = document.RootElement;

                switch (root.ValueKind)
                {
                    case JsonValueKind.Array:
                        foreach (var element in root.EnumerateArray())
                            mappings.Add(ParseMappingObject(element));
                        break;

                    case JsonValueKind.Object:
                        mappings.Add(ParseMappingObject(root));
                        break;

                    default:
                        throw new InvalidTemplateException(EngineMessages.MappingsMustBeJson(Preview(part)));
                }
            }

            return mappings;
        }
        catch (InvalidTemplateException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new InvalidTemplateException(
                EngineMessages.InvalidMappingsJson(
                    exception.Message,
                    Preview(string.Join(MappingJsonConstants.PreviewSeparator, parts))));
        }
    }

    private static ColumnMapping ParseMappingObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidTemplateException(EngineMessages.InvalidMappingFormat);

        foreach (var property in element.EnumerateObject())
        {
            if (!KnownProperties.Contains(property.Name))
                throw new InvalidTemplateException(EngineMessages.UnknownMappingProperty(property.Name));
        }

        if (!TryGetString(element, MappingJsonConstants.Properties.ColumnName, out var columnName) ||
            string.IsNullOrWhiteSpace(columnName))
        {
            throw new InvalidTemplateException(EngineMessages.ColumnNameRequired);
        }

        if (!TryGetString(element, MappingJsonConstants.Properties.FieldName, out var fieldName) ||
            string.IsNullOrWhiteSpace(fieldName))
        {
            throw new InvalidTemplateException(EngineMessages.FieldNameRequired);
        }

        var required = false;
        if (TryGetProperty(element, MappingJsonConstants.Properties.Required, out var requiredElement))
            required = ReadBool(requiredElement);

        var unique = false;
        if (TryGetProperty(element, MappingJsonConstants.Properties.Unique, out var uniqueElement))
            unique = ReadBool(uniqueElement);

        string? regex = null;
        if (TryGetString(element, MappingJsonConstants.Properties.Regex, out var regexValue) &&
            !string.IsNullOrWhiteSpace(regexValue))
            regex = regexValue;

        int? minLength = null;
        if (TryGetProperty(element, MappingJsonConstants.Properties.MinLength, out var minLengthElement))
            minLength = ReadInt(minLengthElement);

        int? maxLength = null;
        if (TryGetProperty(element, MappingJsonConstants.Properties.MaxLength, out var maxLengthElement))
            maxLength = ReadInt(maxLengthElement);

        decimal? min = null;
        if (TryGetProperty(element, MappingJsonConstants.Properties.Min, out var minElement))
            min = ReadDecimal(minElement);

        decimal? max = null;
        if (TryGetProperty(element, MappingJsonConstants.Properties.Max, out var maxElement))
            max = ReadDecimal(maxElement);

        return new ColumnMapping(
            columnName.Trim(),
            fieldName.Trim(),
            required,
            unique,
            regex,
            minLength,
            maxLength,
            min,
            max);
    }

    private static bool ReadBool(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(element.GetString(), out var b) && b,
            _ => false
        };

    private static int? ReadInt(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetInt32(out var i) => i,
            JsonValueKind.String when int.TryParse(element.GetString(), out var i) => i,
            _ => null
        };

    private static decimal? ReadDecimal(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(element.GetString(), out var d) => d,
            _ => null
        };

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            value = property.Value;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string? value)
    {
        if (!TryGetProperty(element, propertyName, out var propertyValue))
        {
            value = null;
            return false;
        }

        value = propertyValue.ValueKind == JsonValueKind.String
            ? propertyValue.GetString()
            : propertyValue.ToString();
        return true;
    }

    private static bool IsAutoMapToken(string value) =>
        value is MappingJsonConstants.AutoMapAll or MappingJsonConstants.EmptyArray;

    private static string NormalizeJson(string json)
    {
        json = json.Trim().TrimStart(MappingJsonConstants.Bom);
        return json
            .Replace(MappingJsonConstants.LeftDoubleQuote, MappingJsonConstants.AsciiDoubleQuote)
            .Replace(MappingJsonConstants.RightDoubleQuote, MappingJsonConstants.AsciiDoubleQuote)
            .Replace(MappingJsonConstants.LeftSingleQuote, MappingJsonConstants.AsciiSingleQuote)
            .Replace(MappingJsonConstants.RightSingleQuote, MappingJsonConstants.AsciiSingleQuote);
    }

    private static string Preview(string value) =>
        value.Length <= MappingJsonConstants.PreviewMaxLength
            ? value
            : value[..MappingJsonConstants.PreviewMaxLength] + MappingJsonConstants.PreviewEllipsis;
}
