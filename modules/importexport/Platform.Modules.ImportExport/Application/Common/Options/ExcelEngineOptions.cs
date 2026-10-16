using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Application.Common.Options;

/// <summary>
/// Runtime tunables for import/export (bound from config section <see cref="SectionName"/>).
/// Prefer changing these over hard-coding limits in processors.
/// </summary>
public sealed class ExcelEngineOptions
{
    /// <summary>Configuration section name (<c>appsettings.json</c>).</summary>
    public const string SectionName = "ExcelEngine";

    /// <summary>Max uploaded Excel/CSV size in bytes.</summary>
    public long MaxFileSizeBytes { get; init; } = ExcelEngineDefaults.MaxFileSizeBytes;

    /// <summary>Hard cap on processed data rows (import or export).</summary>
    public int MaxRowCount { get; init; } = ExcelEngineDefaults.MaxRowCount;

    /// <summary>Hard cap on columns detected in a sheet / mappings count.</summary>
    public int MaxColumnCount { get; init; } = ExcelEngineDefaults.MaxColumnCount;

    /// <summary>
    /// Soft timeout for one import (stream + validate + persist).
    /// <c>0</c> = disabled (only client / host abort).
    /// </summary>
    public int ImportTimeoutSeconds { get; init; } = ExcelEngineDefaults.ImportTimeoutSeconds;

    /// <summary>
    /// Soft timeout for one export. <c>0</c> = disabled.
    /// </summary>
    public int ExportTimeoutSeconds { get; init; } = ExcelEngineDefaults.ExportTimeoutSeconds;

    /// <summary>
    /// HTTP/Kestrel request timeout in seconds. <c>0</c> = auto
    /// (<c>max(Import,Export) + HostTimeoutPaddingSeconds</c>).
    /// </summary>
    public int HostRequestTimeoutSeconds { get; init; }

    /// <summary>Padding when auto-computing host timeout (must exceed engine soft timeout).</summary>
    public int HostTimeoutPaddingSeconds { get; init; } = ExcelEngineDefaults.HostTimeoutPaddingSeconds;

    /// <summary>
    /// Flush valid rows to <see cref="Interfaces.IImportDataPersister"/> every N rows, then clear the batch (NFR-001).
    /// </summary>
    public int PersistBatchSize { get; init; } = ExcelEngineDefaults.PersistBatchSize;

    /// <summary>When false, results never include row payloads (counts + errorSummary only).</summary>
    public bool IncludeRowsInResponse { get; init; } = true;

    /// <summary>
    /// If valid rows exceed this, response <c>rows</c> are cleared (<c>rowsOmitted</c>).
    /// Counts and errorSummary remain.
    /// </summary>
    public int IncludeRowsMaxRowCount { get; init; } = ExcelEngineDefaults.IncludeRowsMaxRowCount;

    /// <summary>Root folder for template files when using Db/Folder store (<c>Location</c>).</summary>
    public string TemplateFilesRoot { get; init; } = "template-files";

    /// <summary>Allowed upload extensions (e.g. .xlsx, .csv).</summary>
    public string[] SupportedExtensions { get; init; } =
    [
        EngineMessages.XlsxExtension,
        EngineMessages.CsvExtension
    ];

    /// <summary>
    /// Resolves host HTTP timeout so it stays above engine soft timeouts (RISK-001).
    /// </summary>
    public int ResolveHostRequestTimeoutSeconds()
    {
        if (HostRequestTimeoutSeconds > 0)
            return HostRequestTimeoutSeconds;

        var engineSeconds = Math.Max(ImportTimeoutSeconds, ExportTimeoutSeconds);
        if (engineSeconds <= 0)
            return ExcelEngineDefaults.ImportTimeoutSeconds + Math.Max(0, HostTimeoutPaddingSeconds);

        return engineSeconds + Math.Max(0, HostTimeoutPaddingSeconds);
    }
}
