using System.Text.Json;
using System.Text.Json.Serialization;
using Platform.Modules.ImportExport.Application.Templates.Models;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates;

/// <summary>
/// Serializes and deserializes template layout JSON (<c>MappingsJson</c>).
/// Shared by template stores, API helpers, and EF adapters.
/// </summary>
public static class TemplateLayoutParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Persists layout into MappingsJson. Sheet name(s) belong in JSON only —
    /// multi-sheet → <c>sheets[]</c>; legacy one named sheet → root <c>sheetName</c>;
    /// plain mapping array → all workbook sheets.
    /// </summary>
    public static string SerializeLayout(
        IReadOnlyList<ColumnMapping> mappings,
        IReadOnlyList<SheetTemplate>? sheets = null,
        string? sheetName = null)
    {
        if (sheets is { Count: > 0 })
        {
            return JsonSerializer.Serialize(
                new TemplateMappingDocument
                {
                    Sheets = sheets.ToList(),
                    Mappings = mappings.Count > 0 ? mappings.ToList() : null
                },
                JsonOptions);
        }

        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            return JsonSerializer.Serialize(
                new TemplateMappingDocument
                {
                    SheetName = sheetName.Trim(),
                    Mappings = mappings.ToList()
                },
                JsonOptions);
        }

        return JsonSerializer.Serialize(mappings, JsonOptions);
    }

    public static (IReadOnlyList<ColumnMapping> Mappings, IReadOnlyList<SheetTemplate> Sheets, string? SheetName)
        DeserializeLayout(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ([], [], null);

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var mappings = (JsonSerializer.Deserialize<List<ColumnMapping>>(json, JsonOptions) ?? [])
                .Select(NormalizeMapping)
                .ToList();
            return (mappings, [], null);
        }

        var body = JsonSerializer.Deserialize<TemplateMappingDocument>(json, JsonOptions)
                   ?? new TemplateMappingDocument();
        return NormalizeLayout(body.Mappings ?? [], body.Sheets ?? [], body.SheetName);
    }

    /// <summary>
    /// Chuẩn hóa layout sau deserialize.
    /// FE gửi Excel 1 sheet dạng <c>{ sheets: [{ sheetName, mappings }] }</c> — promote lên root.
    /// </summary>
    internal static (IReadOnlyList<ColumnMapping> Mappings, IReadOnlyList<SheetTemplate> Sheets, string? SheetName)
        NormalizeLayout(
            IReadOnlyList<ColumnMapping> mappings,
            IReadOnlyList<SheetTemplate> sheets,
            string? sheetName)
    {
        var mappingList = mappings.Count == 0
            ? []
            : mappings.Select(NormalizeMapping).ToList();
        var sheetList = sheets.Count == 0
            ? []
            : sheets.Select(NormalizeSheet).ToList();
        var resolvedSheetName = string.IsNullOrWhiteSpace(sheetName) ? null : sheetName.Trim();

        if (mappingList.Count == 0 && sheetList.Count == 1)
        {
            var sheet = sheetList[0];
            var sheetMappings = sheet.Mappings?.ToList() ?? [];
            if (sheetMappings.Count > 0)
            {
                mappingList = sheetMappings;
                resolvedSheetName ??= string.IsNullOrWhiteSpace(sheet.SheetName)
                    ? null
                    : sheet.SheetName.Trim();
                sheetList = [];
            }
        }

        return (mappingList, sheetList, resolvedSheetName);
    }

    private static ColumnMapping NormalizeMapping(ColumnMapping mapping) =>
        mapping with
        {
            ColumnName = mapping.ColumnName.Trim(),
            FieldName = mapping.FieldName.Trim()
        };

    private static SheetTemplate NormalizeSheet(SheetTemplate sheet) =>
        new()
        {
            SheetName = string.IsNullOrWhiteSpace(sheet.SheetName) ? sheet.SheetName : sheet.SheetName.Trim(),
            Mappings = sheet.Mappings.Select(NormalizeMapping).ToList(),
            StartRow = sheet.StartRow,
            HeaderRow = sheet.HeaderRow,
            RowCount = sheet.RowCount
        };
}
