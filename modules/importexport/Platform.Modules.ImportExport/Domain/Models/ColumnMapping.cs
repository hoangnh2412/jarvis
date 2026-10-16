namespace Platform.Modules.ImportExport.Domain.Models;

/// <param name="ColumnName">Excel header name.</param>
/// <param name="FieldName">Output field name.</param>
/// <param name="Required">When true, empty/missing cell fails that row.</param>
/// <param name="Unique">When true, duplicate values (in-file / store) fail that row.</param>
/// <param name="Regex">Optional .NET regex pattern the cell string must match.</param>
/// <param name="MinLength">Optional min string length.</param>
/// <param name="MaxLength">Optional max string length.</param>
/// <param name="Min">Optional numeric minimum (inclusive).</param>
/// <param name="Max">Optional numeric maximum (inclusive).</param>
public sealed record ColumnMapping(
    string ColumnName,
    string FieldName,
    bool Required = false,
    bool Unique = false,
    string? Regex = null,
    int? MinLength = null,
    int? MaxLength = null,
    decimal? Min = null,
    decimal? Max = null);
