namespace Platform.Modules.ImportExport.Application.Import.Models;

/// <summary>Import result for one worksheet.</summary>
public sealed class SheetImportResult
{
    public required string SheetName { get; init; }

    /// <summary>
    /// Valid rows included in the payload (may be empty when omitted for RAM — see <see cref="ValidRowCount"/>).
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; init; } =
        Array.Empty<IReadOnlyDictionary<string, object?>>();

    /// <summary>Aggregated validation errors for this sheet only (no <c>sheetName</c> on each item).</summary>
    public IReadOnlyList<ImportErrorSummary> ErrorSummary { get; init; } =
        Array.Empty<ImportErrorSummary>();

    public int ProcessedRowCount { get; init; }

    public int FailedRowCount { get; init; }

    /// <summary>Valid rows processed (independent of whether <see cref="Rows"/> was kept in memory).</summary>
    public int ValidRowCount { get; init; }

    /// <summary>True when valid rows were dropped from the response to protect RAM/JSON size.</summary>
    public bool RowsOmitted { get; init; }
}
