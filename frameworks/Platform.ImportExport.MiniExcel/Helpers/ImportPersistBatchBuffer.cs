using Microsoft.Extensions.Logging;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Buffer batch persist + (optional) rows trả về HTTP/DLL khi import streaming (NFR-001).
/// </summary>
/// <remarks>
/// <para>
/// Mỗi row hợp lệ → <see cref="AcceptAsync"/>; đủ <c>PersistBatchSize</c> →
/// <see cref="IImportDataPersister.SaveAsync"/> rồi <c>Clear</c> batch để không giữ cả file trong RAM.
/// </para>
/// <para>
/// Response rows: bật bằng <c>IncludeRowsInResponse</c>; vượt <c>IncludeRowsMaxRowCount</c>
/// → bỏ buffer, set <see cref="RowsOmittedInResponse"/>.
/// </para>
/// </remarks>
internal sealed class ImportPersistBatchBuffer
{
    private readonly IImportDataPersister _dataPersister;
    private readonly int _batchSize;
    private readonly bool _persistEnabled;
    private readonly ILogger _logger;
    private readonly List<IReadOnlyDictionary<string, object?>> _batch;
    private readonly List<IReadOnlyDictionary<string, object?>>? _responseRows;
    private bool _collectResponseRows;
    private bool _rowsOmitted;

    public ImportPersistBatchBuffer(
        IImportDataPersister dataPersister,
        int batchSize,
        bool includeRowsInResponse,
        int includeRowsMaxRowCount,
        bool persistEnabled = true,
        ILogger? logger = null)
    {
        _dataPersister = dataPersister;
        _batchSize = Math.Max(1, batchSize);
        _persistEnabled = persistEnabled;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        MaxResponseRows = Math.Max(0, includeRowsMaxRowCount);
        _collectResponseRows = includeRowsInResponse;
        _rowsOmitted = !includeRowsInResponse;
        _batch = new List<IReadOnlyDictionary<string, object?>>(
            Math.Min(_batchSize, ExcelEngineDefaults.ListCapacityHint));
        _responseRows = includeRowsInResponse
            ? new List<IReadOnlyDictionary<string, object?>>(
                Math.Min(ExcelEngineDefaults.ListCapacityHint, Math.Max(1, MaxResponseRows)))
            : null;
    }

    /// <summary>Valid rows accepted so far (independent of response payload).</summary>
    public int ValidRowCount { get; private set; }

    /// <summary>Configured cap for rows kept in the HTTP/DLL response.</summary>
    public int MaxResponseRows { get; }

    /// <summary>
    /// Adds a valid row to the persist batch (and response buffer when enabled).
    /// Returns saved count when a batch flush occurs; otherwise 0.
    /// </summary>
    public async Task<int> AcceptAsync(
        IReadOnlyDictionary<string, object?> mapped,
        TemplateDefinition templateDef,
        List<ImportRowError> errors,
        CancellationToken cancellationToken)
    {
        ValidRowCount++;
        _batch.Add(mapped);
        TryCollectForResponse(mapped);

        if (_batch.Count < _batchSize)
            return 0;

        var saved = await FlushAsync(templateDef, errors, cancellationToken);
        _batch.Clear();
        return saved;
    }

    /// <summary>Flushes any remaining rows in the batch.</summary>
    public async Task<int> FlushRemainingAsync(
        TemplateDefinition templateDef,
        List<ImportRowError> errors,
        CancellationToken cancellationToken)
    {
        if (_batch.Count == 0)
            return 0;

        var saved = await FlushAsync(templateDef, errors, cancellationToken);
        _batch.Clear();
        _batch.TrimExcess();
        return saved;
    }

    /// <summary>Builds the response row list (empty when omitted for RAM).</summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> BuildResponseRows()
    {
        if (_rowsOmitted || _responseRows is null)
            return Array.Empty<IReadOnlyDictionary<string, object?>>();

        return _responseRows;
    }

    /// <summary>True when valid rows existed but were not included in the response payload.</summary>
    public bool RowsOmittedInResponse => ValidRowCount > 0 && BuildResponseRows().Count == 0;

    private void TryCollectForResponse(IReadOnlyDictionary<string, object?> mapped)
    {
        if (!_collectResponseRows || _responseRows is null)
            return;

        if (ValidRowCount <= MaxResponseRows)
        {
            _responseRows.Add(mapped);
            return;
        }

        _responseRows.Clear();
        _responseRows.TrimExcess();
        _collectResponseRows = false;
        _rowsOmitted = true;
    }

    /// <summary>
    /// Gọi persister với batch hiện tại; lỗi persist (DB unique, …) gộp vào <paramref name="errors"/>.
    /// </summary>
    private async Task<int> FlushAsync(
        TemplateDefinition templateDef,
        List<ImportRowError> errors,
        CancellationToken cancellationToken)
    {
        if (!_persistEnabled)
            return 0;

        var batchCount = _batch.Count;
        // DefaultImportDataPersister = no-op (SavedCount = batch.Count); consumer thay bằng MyPersister
        var persist = await _dataPersister.SaveAsync(_batch, templateDef, cancellationToken);
        if (persist.Errors.Count > 0)
            errors.AddRange(persist.Errors);

        _logger.LogInformation(
            EngineLogMessages.PersistBatchCompleted,
            templateDef.Code,
            templateDef.EntityKey,
            batchCount,
            persist.SavedCount,
            persist.Errors.Count);

        return persist.SavedCount;
    }
}
