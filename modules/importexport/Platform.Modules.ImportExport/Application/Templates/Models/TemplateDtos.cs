using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates.Models;

public class TemplateSummary
{
    public Guid TenantId { get; init; }

    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string FileName { get; init; }
    public string? Location { get; init; }
    public int HeaderRow { get; init; }
    public int StartRow { get; init; }
    public int? RowCount { get; init; }
    public string? SheetName { get; init; }
    public required IReadOnlyList<ColumnMapping> Mappings { get; init; }
    public IReadOnlyList<SheetTemplate> Sheets { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Map summary store → <see cref="TemplateConfiguration"/> cho <c>ImportWork</c>
    /// (dùng bởi <c>ImportService</c> trước khi vào processor).
    /// </summary>
    public TemplateConfiguration ToImportConfiguration() => new()
    {
        Mappings = Mappings,
        HeaderRow = HeaderRow,
        StartRow = StartRow,
        RowCount = RowCount,
        SheetName = SheetName,
        Sheets = Sheets
    };
}

public sealed class TemplateDetail : TemplateSummary
{
    public required byte[] Content { get; init; }
}