namespace Platform.Modules.ImportExport.Domain.Models;

/// <summary>
/// Portable template definition for <see cref="Interfaces.ITemplateStore"/>.
/// File bytes may be inline (<see cref="Content"/>) or resolved from <see cref="Location"/>.
/// </summary>
public sealed class TemplateDefinition
{
    public Guid TenantId { get; init; }

    public required string Code { get; init; }

    /// <summary>Registry key for module persist routing — from <see cref="Application.Import.Models.ImportRequest"/> (host).</summary>
    public string? EntityKey { get; init; }

    public required string Name { get; init; }
    public required string FileName { get; init; }

    public string? Location { get; init; }
    public byte[]? Content { get; init; }

    /// <summary>Legacy flat mappings (when <see cref="Sheets"/> empty).</summary>
    public IReadOnlyList<ColumnMapping> Mappings { get; init; } = [];

    public int HeaderRow { get; init; } = 1;
    public int StartRow { get; init; } = 2;
    public int? RowCount { get; init; }
    /// <summary>
    /// Legacy single-sheet target (null = all workbook sheets when <see cref="Sheets"/> empty).
    /// Persisted inside <c>MappingsJson</c>, not as a DB column.
    /// </summary>
    public string? SheetName { get; init; }

    /// <summary>Multi-sheet layout. Prefer this over root Mappings/SheetName.</summary>
    public IReadOnlyList<SheetTemplate> Sheets { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

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