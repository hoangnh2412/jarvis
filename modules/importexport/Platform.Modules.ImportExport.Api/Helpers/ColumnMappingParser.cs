using Microsoft.Extensions.Primitives;

namespace Platform.Modules.ImportExport.API.Helpers;

/// <summary>
/// Parses template layout JSON: legacy mapping array OR document with sheetName / sheets[].
/// </summary>
public static class ColumnMappingParser
{
    public static (IReadOnlyList<ColumnMapping> Mappings, IReadOnlyList<SheetTemplate> Sheets, string? SheetName)
        ParseLayout(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ([], [], null);

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);
        if (sheets.Count > 0)
        {
            foreach (var sheet in sheets)
                MappingValidator.Validate(sheet.Mappings, requireAtLeastOne: true);
            return (mappings, sheets, null);
        }

        MappingValidator.Validate(mappings, requireAtLeastOne: true);
        return (mappings, sheets, sheetName);
    }

    public static (IReadOnlyList<ColumnMapping> Mappings, IReadOnlyList<SheetTemplate> Sheets, string? SheetName)
        ParseLayout(StringValues values)
    {
        var parts = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
        if (parts.Length == 0)
            return ([], [], null);

        if (parts.Length == 1)
            return ParseLayout(parts[0]);

        var mappings = MappingValidator.ParseAndValidate(parts);
        return (mappings, [], null);
    }
}
