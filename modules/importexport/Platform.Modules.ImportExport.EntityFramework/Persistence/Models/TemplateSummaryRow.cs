namespace Platform.Modules.ImportExport.EntityFramework.Persistence.Models;

/// <summary>Projection row used when listing templates without loading full entities.</summary>
internal sealed class TemplateSummaryRow
{
    public Guid TenantId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string FileName { get; init; }
    public string? Location { get; init; }
    public required string MappingsJson { get; init; }
    public int HeaderRow { get; init; }
    public int StartRow { get; init; }
    public int? RowCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
