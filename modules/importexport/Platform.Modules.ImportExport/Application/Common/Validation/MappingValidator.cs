using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Exceptions;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>
/// Domain + JSON mapping validation shared by API host, template store, and processors.
/// </summary>
public static class MappingValidator
{
    public static IReadOnlyList<ColumnMapping> ParseAndValidate(
        string? json,
        bool requireAtLeastOne = false)
    {
        var mappings = ColumnMappingJsonParser.Parse(json);
        Validate(mappings, requireAtLeastOne);
        return mappings;
    }

    public static IReadOnlyList<ColumnMapping> ParseAndValidate(
        IEnumerable<string?> values,
        bool requireAtLeastOne = false)
    {
        var mappings = ColumnMappingJsonParser.Parse(values);
        Validate(mappings, requireAtLeastOne);
        return mappings;
    }

    public static void Validate(IReadOnlyCollection<ColumnMapping> mappings, bool requireAtLeastOne = false)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        if (requireAtLeastOne && mappings.Count == 0)
            throw new InvalidTemplateException(EngineMessages.MappingsRequired);

        if (mappings.Count == 0)
            return;

        var columnNames = new HashSet<string>(mappings.Count, StringComparer.OrdinalIgnoreCase);
        var fieldNames = new HashSet<string>(mappings.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.ColumnName))
                throw new InvalidTemplateException(EngineMessages.ColumnNameRequired);

            if (string.IsNullOrWhiteSpace(mapping.FieldName))
                throw new InvalidTemplateException(EngineMessages.FieldNameRequired);

            if (!columnNames.Add(mapping.ColumnName.Trim()))
                throw new InvalidTemplateException(EngineMessages.ColumnNameUnique);

            if (!fieldNames.Add(mapping.FieldName.Trim()))
                throw new InvalidTemplateException(EngineMessages.FieldNameUnique);

            if (mapping.MinLength is { } minLen &&
                mapping.MaxLength is { } maxLen &&
                minLen > maxLen)
            {
                throw new InvalidTemplateException(
                    EngineMessages.MappingMinLengthExceedsMaxLength(mapping.FieldName));
            }
        }
    }
}
