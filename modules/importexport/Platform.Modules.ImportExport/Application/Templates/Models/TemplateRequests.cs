using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates.Models;

public sealed class CreateTemplateRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
    public IReadOnlyList<ColumnMapping> Mappings { get; init; } = [];
    public IReadOnlyList<SheetTemplate> Sheets { get; init; } = [];
    public int HeaderRow { get; init; } = 1;
    public int StartRow { get; init; } = 2;
    public int? RowCount { get; init; }
    public string? SheetName { get; init; }
}

public sealed class UpdateTemplateRequest
{
    public string? Name { get; init; }
    public string? FileName { get; init; }
    public byte[]? Content { get; init; }
    public IReadOnlyList<ColumnMapping>? Mappings { get; init; }
    public IReadOnlyList<SheetTemplate>? Sheets { get; init; }
    public int? HeaderRow { get; init; }
    public int? StartRow { get; init; }
    public int? RowCount { get; init; }
    public string? SheetName { get; init; }
}

public sealed class UpdateTemplateMetadataRequest
{
    public required string Name { get; init; }
}

public sealed class UpdateTemplateConfigurationRequest
{
    public string? FileName { get; init; }
    public byte[]? Content { get; init; }
    public IReadOnlyList<ColumnMapping>? Mappings { get; init; }
    public IReadOnlyList<SheetTemplate>? Sheets { get; init; }
    public int? HeaderRow { get; init; }
    public int? StartRow { get; init; }
    public int? RowCount { get; init; }
    public string? SheetName { get; init; }
}