using System.Text.Json.Serialization;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates.Models;

/// <summary>
/// Stored in MappingsJson — legacy mapping array, optional root sheetName, or multi-sheet document.
/// Sheet names live here (not as a separate DB column).
/// </summary>
internal sealed class TemplateMappingDocument
{
    [JsonPropertyName("sheetName")]
    public string? SheetName { get; set; }

    [JsonPropertyName("mappings")]
    public List<ColumnMapping>? Mappings { get; set; }

    [JsonPropertyName("sheets")]
    public List<SheetTemplate>? Sheets { get; set; }
}
