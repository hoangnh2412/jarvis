namespace Platform.Modules.ImportExport.Application.Schemas.Models;

public sealed record ImportExportEntitySummary(string Key, string DisplayName);

public sealed record ImportExportFieldSchema(
    string Name,
    string DisplayName,
    string DataType,
    string ClrType,
    bool Required,
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    string? Regex,
    bool IsNumeric);

public sealed record RegisteredImportExportEntity(string Key, Type RowType, string DisplayName);
