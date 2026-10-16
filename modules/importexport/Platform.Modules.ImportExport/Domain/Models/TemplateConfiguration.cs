using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Domain.Models;

/// <summary>One sheet's import layout (mappings + row bounds).</summary>
public sealed class SheetTemplate
{
    /// <summary>Null/empty = apply this mapping to every sheet in the workbook (only when Sheets has a single entry).</summary>
    public string? SheetName { get; init; }

    public IReadOnlyList<ColumnMapping> Mappings { get; init; } = [];

    public int StartRow { get; init; } = 2;

    public int? HeaderRow { get; init; }

    public int? RowCount { get; init; }

    public int ResolveHeaderRow() => HeaderRow ?? Math.Max(1, StartRow - 1);
}

/// <summary>Defines how an import workbook is consumed (1 sheet, N sheets, or all sheets).</summary>
public sealed class TemplateConfiguration
{
    /// <summary>Legacy single-sheet mappings (used when <see cref="Sheets"/> is empty).</summary>
    public IReadOnlyCollection<ColumnMapping> Mappings { get; init; } = Array.Empty<ColumnMapping>();

    public int StartRow { get; init; } = 2;
    public int? HeaderRow { get; init; }
    public int? RowCount { get; init; }

    /// <summary>Legacy: target one sheet; null = first sheet only when Sheets empty. Prefer <see cref="Sheets"/>.</summary>
    public string? SheetName { get; init; }

    /// <summary>
    /// Multi-sheet config. Each entry can target a named sheet with its own mappings.
    /// One entry with <c>SheetName = null</c> → import <b>all</b> workbook sheets with that mapping.
    /// </summary>
    public IReadOnlyList<SheetTemplate> Sheets { get; init; } = [];

    public InvalidRowBehavior InvalidRowBehavior { get; init; } = InvalidRowBehavior.Continue;

    /// <summary>
    /// Chuẩn hóa legacy fields thành sheet plans cho import processor / expander.
    /// </summary>
    /// <remarks>
    /// <c>Sheets</c> có phần tử → dùng nguyên; ngược lại bọc <c>Mappings</c> + <c>SheetName</c> thành 1 <see cref="SheetTemplate"/>.
    /// </remarks>
    public IReadOnlyList<SheetTemplate> ResolveSheetPlans()
    {
        if (Sheets.Count > 0)
            return Sheets;

        return
        [
            new SheetTemplate
            {
                SheetName = SheetName,
                Mappings = Mappings is IReadOnlyList<ColumnMapping> list ? list : Mappings.ToList(),
                StartRow = StartRow,
                HeaderRow = HeaderRow,
                RowCount = RowCount
            }
        ];
    }
}