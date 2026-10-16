using Platform.Modules.ImportExport.Application.Common.Validation;

namespace Platform.Modules.ImportExport.Application.Import.Models;

public sealed class ImportResult
{
    /// <summary>Per-sheet results (valid rows + <see cref="SheetImportResult.ErrorSummary"/>).</summary>
    public IReadOnlyList<SheetImportResult> Sheets { get; init; } = Array.Empty<SheetImportResult>();

    /// <summary>Flat cell errors (DLL consumers). HTTP API exposes summaries under <see cref="Sheets"/> only.</summary>
    public IReadOnlyList<ImportRowError> Errors { get; init; } = Array.Empty<ImportRowError>();

    public int ProcessedRowCount { get; init; }

    public int FailedRowCount { get; init; }

    public int SavedCount { get; init; }

    /// <summary>Total valid rows across all sheets (not tied to response payload size).</summary>
    public int ValidRowCount => Sheets.Sum(s => s.ValidRowCount);

    public static ImportResult From(
        IReadOnlyList<SheetImportResult> sheets,
        IReadOnlyList<ImportRowError> errors,
        int savedCount = 0)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentNullException.ThrowIfNull(errors);

        var processed = sheets.Sum(s => s.ProcessedRowCount);
        var enriched = AttachErrorSummaries(sheets, errors);

        return new ImportResult
        {
            Sheets = enriched,
            Errors = errors,
            ProcessedRowCount = processed,
            FailedRowCount = enriched.Sum(s => s.FailedRowCount),
            SavedCount = savedCount
        };
    }

    private static IReadOnlyList<SheetImportResult> AttachErrorSummaries(
        IReadOnlyList<SheetImportResult> sheets,
        IReadOnlyList<ImportRowError> errors)
    {
        if (sheets.Count == 0)
            return sheets;

        if (errors.Count == 0)
            return sheets;

        var bySheet = errors
            .GroupBy(e => e.SheetName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ImportRowError>)g.ToList(), StringComparer.OrdinalIgnoreCase);

        var result = new List<SheetImportResult>(sheets.Count);
        foreach (var sheet in sheets)
        {
            if (!bySheet.TryGetValue(sheet.SheetName, out var sheetErrors))
                sheetErrors = [];

            var aggregated = ImportErrorAggregator.Aggregate(sheetErrors);
            result.Add(new SheetImportResult
            {
                SheetName = sheet.SheetName,
                Rows = sheet.Rows,
                ErrorSummary = aggregated.Summaries,
                ProcessedRowCount = sheet.ProcessedRowCount,
                FailedRowCount = sheet.FailedRowCount > 0
                    ? sheet.FailedRowCount
                    : aggregated.FailedRowCount,
                ValidRowCount = sheet.ValidRowCount,
                RowsOmitted = sheet.RowsOmitted
            });
        }

        return result;
    }
}
