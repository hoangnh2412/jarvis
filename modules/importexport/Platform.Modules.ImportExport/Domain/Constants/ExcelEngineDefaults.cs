namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>
/// Shared numeric defaults for the engine (avoid magic numbers in processors / host).
/// Overridable runtime values live in <see cref="Models.ExcelEngineOptions"/>.
/// </summary>
public static class ExcelEngineDefaults
{
    /// <summary>Default soft import timeout (seconds).</summary>
    public const int ImportTimeoutSeconds = 300;

    /// <summary>Default soft export timeout (seconds).</summary>
    public const int ExportTimeoutSeconds = 300;

    /// <summary>Extra seconds added on top of engine timeout for HTTP/Kestrel host policy.</summary>
    public const int HostTimeoutPaddingSeconds = 60;

    /// <summary>Default persist batch size (rows) before flush + clear.</summary>
    public const int PersistBatchSize = 2_000;

    /// <summary>Default max rows kept in import HTTP/DLL response payload.</summary>
    public const int IncludeRowsMaxRowCount = 10_000;

    /// <summary>Hint for pre-sizing small lists (batches / response buffer).</summary>
    public const int ListCapacityHint = 256;

    /// <summary>Minimum valid Excel header row (1-based).</summary>
    public const int MinHeaderRow = 1;

    /// <summary>Minimum valid Excel data start row (1-based); typically header+1.</summary>
    public const int MinStartRow = 2;

    /// <summary>Default max rows per import/export.</summary>
    public const int MaxRowCount = 1_000_000;

    /// <summary>Default max columns per sheet.</summary>
    public const int MaxColumnCount = 500;

    /// <summary>Default max upload size (50 MB).</summary>
    public const long MaxFileSizeBytes = 50L * 1024 * 1024;

    /// <summary>
    /// Excel column alphabet size (A–Z). Used by A1 address encoding — not a business limit.
    /// </summary>
    public const int ExcelColumnAlphabetSize = 26;

    /// <summary>Cap when concatenating xlsx XML entries for placeholder scan.</summary>
    public const int ZipXmlScanBufferCapBytes = 512 * 1024;

    /// <summary>Extra capacity when cloning template bytes for OpenXml edit.</summary>
    public const int TemplateEditStreamPaddingBytes = 4 * 1024;
}