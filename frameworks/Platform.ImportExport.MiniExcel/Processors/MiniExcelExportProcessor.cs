using System.Diagnostics;
using Platform.ImportExport.MiniExcel.Helpers;
using Platform.ImportExport.MiniExcel.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace Platform.ImportExport.MiniExcel.Processors;

/// <summary>
/// Processor export mặc định (MiniExcel) — NFR-001 / RISK-001.
/// </summary>
/// <remarks>
/// <para><b>Hai nhánh</b></para>
/// <list type="bullet">
/// <item><b>Có TemplateContent</b> — inject/validate <c>{{rows.FieldName}}</c> rồi <c>SaveAsByTemplate</c> (giữ style).</item>
/// <item><b>Không có template bytes</b> — <c>SaveAsAsync</c> plain (header = ColumnName), stream IEnumerable.</item>
/// </list>
/// Soft timeout: <see cref="ExcelEngineOptions.ExportTimeoutSeconds"/>.
/// </remarks>
public sealed class MiniExcelExportProcessor(
    IOptions<ExcelEngineOptions> options,
    ILogger<MiniExcelExportProcessor> logger) : IExportProcessor
{
    private readonly ExcelEngineOptions _options = options.Value;

    /// <summary>Cấu hình OpenXML cho export plain (không table style / auto filter).</summary>
    private static readonly OpenXmlConfiguration PlainExportConfiguration = new()
    {
        TableStyles = TableStyles.None,
        AutoFilter = false
    };

    /// <summary>
    /// Cấu hình template: thiếu placeholder bắt buộc → MiniExcel lỗi (không ignore).
    /// </summary>
    private static readonly OpenXmlConfiguration TemplateExportConfiguration = new()
    {
        IgnoreTemplateParameterMissing = false
    };

    /// <inheritdoc />
    /// <remarks>
    /// Validate mappings + cột trước; soft timeout bọc <see cref="ExportCoreAsync{T}"/>.
    /// </remarks>
    public async Task<ExportResult> ExportAsync<T>(
        ExportWork<T> work,
        CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(work.Data);
        ArgumentException.ThrowIfNullOrWhiteSpace(work.FileName);
        MappingValidator.Validate(work.Mappings, requireAtLeastOne: true);

        if (work.Mappings.Count > _options.MaxColumnCount)
        {
            throw new InvalidTemplateException(
                EngineMessages.ColumnLimitExceeded(work.Mappings.Count, _options.MaxColumnCount));
        }

        if (work.RowCount is <= 0)
            throw new InvalidTemplateException(EngineMessages.RowCountInvalid());

        // Soft timeout từ ExcelEngineOptions.ExportTimeoutSeconds (0 = tắt)
        try
        {
            return await OperationTimeoutScope.RunAsync(
                _options.ExportTimeoutSeconds,
                "Export",
                cancellationToken,
                ct => ExportCoreAsync(work, ct));
        }
        catch (OperationTimeoutException ex)
        {
            logger.LogWarning(
                ex,
                EngineLogMessages.OperationTimeout,
                ex.OperationName,
                ex.TimeoutSeconds);
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation(EngineLogMessages.ExportCanceled, work.FileName);
            throw;
        }
    }

    /// <summary>
    /// Core export: chọn nhánh template vs plain → ghi MemoryStream → <see cref="ExportResult"/>.
    /// </summary>
    private async Task<ExportResult> ExportCoreAsync<T>(
        ExportWork<T> work,
        CancellationToken cancellationToken)
        where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        var mappings = work.Mappings as ColumnMapping[] ?? work.Mappings.ToArray();
        var sheetName = work.SheetName ?? EngineMessages.DefaultExportSheetName;
        var template = work.TemplateContent;
        var maxRows = ResolveMaxRows(work.RowCount);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Export",
            [LogPropertyNames.TemplateCode] = null,
            [LogPropertyNames.FileName] = work.FileName,
            [LogPropertyNames.Phase] = "Export"
        });

        logger.LogInformation(
            EngineLogMessages.ExportStarted,
            work.FileName,
            sheetName,
            mappings.Length);

        try
        {
            await using var stream = new MemoryStream();
            var counter = new RowCounter();

            if (template is { Length: > 0 })
            {
                // Đảm bảo file mẫu có {{rows.FieldName}} — validate sẵn có hoặc inject OpenXML
                var prepared = ExportTemplatePlaceholderInjector.EnsurePlaceholders(
                    template,
                    mappings,
                    headerRow: Math.Max(ExcelEngineDefaults.MinHeaderRow, work.HeaderRow),
                    startRow: Math.Max(ExcelEngineDefaults.MinStartRow, work.StartRow),
                    preferredSheetName: work.SheetName);

                // SaveAsByTemplate có thể enumerate nhiều lần → materialize một lần (giới hạn MiniExcel)
                var rows = ProjectRows(
                    work.Data,
                    mappings,
                    keySelector: static m => m.FieldName,
                    counter,
                    maxRows,
                    work.RowCount,
                    cancellationToken).ToList();

                await stream.SaveAsByTemplateAsync(
                    prepared,
                    new Dictionary<string, object>
                    {
                        [ExportTemplateConstants.RowsCollectionName] = rows
                    },
                    TemplateExportConfiguration,
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Không có file mẫu — stream SaveAs; key dictionary = ColumnName (header Excel)
                var rows = ProjectRows(
                    work.Data,
                    mappings,
                    keySelector: static m => m.ColumnName,
                    counter,
                    maxRows,
                    work.RowCount,
                    cancellationToken);

                await MiniExcelLibs.MiniExcel.SaveAsAsync(
                    stream,
                    rows,
                    sheetName: sheetName,
                    configuration: PlainExportConfiguration,
                    cancellationToken: cancellationToken);
            }

            var result = new ExportResult
            {
                Content = stream.ToArray(),
                FileName = FileNameHelper.EnsureXlsxExtension(work.FileName)
            };

            stopwatch.Stop();
            logger.LogInformation(
                EngineLogMessages.ExportFinished,
                result.FileName,
                sheetName,
                counter.Count,
                result.Content.Length);

            logger.LogInformation(
                EngineLogMessages.ExportDuration,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ExcelEngineException ex)
        {
            logger.LogWarning(ex, EngineLogMessages.ExportFailed, work.FileName, ex.ErrorCode);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, EngineLogMessages.ExportFailedUnexpectedly, work.FileName);
            throw new ExportException(EngineMessages.FailedToExportExcel, exception);
        }
    }

    /// <summary>
    /// Chiếu từng phần tử <typeparamref name="T"/> thành dictionary theo mapping;
    /// enforce <c>min(template.RowCount, MaxRowCount)</c> hoặc chỉ <c>MaxRowCount</c>.
    /// </summary>
    /// <param name="keySelector">
    /// Template path → <c>FieldName</c>; plain SaveAs → <c>ColumnName</c> (làm header).
    /// </param>
    private IEnumerable<Dictionary<string, object?>> ProjectRows<T>(
        IEnumerable<T> data,
        ColumnMapping[] mappings,
        Func<ColumnMapping, string> keySelector,
        RowCounter counter,
        int maxRows,
        int? templateRowCount,
        CancellationToken cancellationToken)
    {
        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryAcceptRow(counter, maxRows, templateRowCount))
                yield break;

            var row = new Dictionary<string, object?>(mappings.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var mapping in mappings)
                row[keySelector(mapping)] = ResolveValue(item, mapping);

            yield return row;
        }
    }

    /// <summary>
    /// Giới hạn số row export: <c>min(template.RowCount, MaxRowCount)</c> hoặc chỉ <c>MaxRowCount</c>.
    /// </summary>
    private int ResolveMaxRows(int? templateRowCount) =>
        templateRowCount is { } requested
            ? Math.Min(requested, _options.MaxRowCount)
            : _options.MaxRowCount;

    /// <summary>
    /// Tăng counter nếu còn slot; chạm cap template → stop êm;
    /// chạm <c>MaxRowCount</c> toàn cục (không có template RowCount) → <see cref="RowLimitExceededException"/>.
    /// </summary>
    private bool TryAcceptRow(RowCounter counter, int maxRows, int? templateRowCount)
    {
        if (counter.Count >= maxRows)
        {
            if (templateRowCount is null)
                throw new RowLimitExceededException(counter.Count + 1, _options.MaxRowCount);
            return false;
        }

        counter.Increment();
        return true;
    }

    /// <summary>
    /// Lấy giá trị một field từ row nguồn: dictionary (API) ưu tiên FieldName rồi ColumnName;
    /// POCO dùng reflection <see cref="PropertyAccessor"/>.
    /// </summary>
    private static object? ResolveValue<T>(T item, ColumnMapping mapping)
    {
        if (item is null)
            return null;

        if (item is IReadOnlyDictionary<string, object?> dictionary)
        {
            if (dictionary.TryGetValue(mapping.FieldName, out var byField))
                return byField;
            if (dictionary.TryGetValue(mapping.ColumnName, out var byColumn))
                return byColumn;
            return null;
        }

        return PropertyAccessor.GetValue(item, mapping.FieldName);
    }
}
