namespace Platform.Modules.ImportExport.Application.Import.Models;

/// <summary>Stable error codes for import row/field validation (DLL + API consumers).</summary>
public static class ImportErrorCodes
{
    public const string FieldRequired = nameof(FieldRequired);
    public const string InvalidRowStructure = nameof(InvalidRowStructure);
    public const string InvalidFormat = nameof(InvalidFormat);
    public const string MaxLengthExceeded = nameof(MaxLengthExceeded);
    public const string MinLengthNotMet = nameof(MinLengthNotMet);
    public const string OutOfRange = nameof(OutOfRange);
    public const string DuplicateValue = nameof(DuplicateValue);
    public const string DuplicateInStore = nameof(DuplicateInStore);
    public const string PersistFailed = nameof(PersistFailed);
}

/// <summary>
/// One import validation failure. Prefer one entry per (row, column) so consumers can highlight cells.
/// </summary>
public sealed record ImportRowError(
    int RowNumber,
    string Message,
    string? ColumnName = null,
    string? FieldName = null,
    string? ErrorCode = null,
    string? SheetName = null);

/// <summary>
/// Aggregated view of <see cref="ImportRowError"/> for the same issue across rows within one sheet.
/// <c>sheetName</c> lives on the parent <c>SheetImportResult</c>, not here.
/// </summary>
public sealed record ImportErrorSummary(
    string Message,
    IReadOnlyList<int> RowNumbers,
    string? ColumnName = null,
    string? FieldName = null,
    string? ErrorCode = null)
{
    public int Count => RowNumbers.Count;
}