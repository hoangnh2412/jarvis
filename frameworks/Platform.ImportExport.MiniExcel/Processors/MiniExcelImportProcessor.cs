using System.Diagnostics;
using Platform.ImportExport.MiniExcel.Helpers;
using Platform.ImportExport.MiniExcel.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniExcelLibs;

namespace Platform.ImportExport.MiniExcel.Processors;

/// <summary>
/// Processor import mặc định (MiniExcel streaming) — NFR-001 / RISK-001.
/// </summary>
/// <remarks>
/// <para><b>Pipeline</b></para>
/// <list type="number">
/// <item><see cref="IImportRequestValidator"/> — extension, size, HeaderRow/StartRow, mappings.</item>
/// <item>Soft timeout (<see cref="ExcelEngineOptions.ImportTimeoutSeconds"/>) qua <see cref="OperationTimeoutScope"/>.</item>
/// <item><see cref="SheetPlanExpander"/> — 1 sheet / N sheet / tất cả sheet.</item>
/// <item>Mỗi sheet: <c>MiniExcelLibs.MiniExcel.QueryAsync</c> stream → map → <see cref="IRowValidator"/> → batch persist.</item>
/// <item>Aggregate <see cref="ImportResult"/> (sheets + errors + savedCount).</item>
/// </list>
/// <para>
/// Không <c>ToList()</c> toàn bộ workbook — chỉ giữ batch RAM theo <c>PersistBatchSize</c>.
/// </para>
/// </remarks>
public sealed class MiniExcelImportProcessor(
    IImportRequestValidator requestValidator,
    IRowValidator rowValidator,
    IImportDataPersister dataPersister,
    IOptions<ExcelEngineOptions> options,
    ILogger<MiniExcelImportProcessor> logger) : IImportProcessor
{
    private readonly ExcelEngineOptions _options = options.Value;

    /// <inheritdoc />
    /// <remarks>
    /// Entry từ <c>ImportService</c> / executor. Validate trước, rồi bọc soft timeout quanh core.
    /// </remarks>
    public async Task<ImportResult> ImportAsync(ImportWork work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);
        requestValidator.Validate(work);

        // Soft timeout từ ExcelEngineOptions.ImportTimeoutSeconds (0 = tắt, chỉ dùng caller token)
        try
        {
            return await OperationTimeoutScope.RunAsync(
                _options.ImportTimeoutSeconds,
                "Import",
                cancellationToken,
                ct => ImportCoreAsync(work, ct));
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
            logger.LogInformation(EngineLogMessages.ImportCanceled, work.FileName);
            throw;
        }
    }

    /// <summary>
    /// Core import sau khi đã link timeout token: expand sheet plans → import từng sheet → aggregate.
    /// </summary>
    private async Task<ImportResult> ImportCoreAsync(ImportWork work, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        // Stop = dừng ngay sheet/row đầu tiên có lỗi; Continue = gom lỗi và xử lý tiếp
        var stopOnError = work.Template.InvalidRowBehavior == InvalidRowBehavior.Stop;

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Import",
            [LogPropertyNames.TemplateCode] = work.TemplateCode,
            [LogPropertyNames.FileName] = work.FileName,
            [LogPropertyNames.Phase] = work.Phase.ToString()
        });

        // Đọc tên sheet từ workbook 1 lần; mỗi target = (plan mappings + tên sheet thực tế)
        var sheetPlans = SheetPlanExpander.Expand(work.FileStream, work.FileName, work.Template);
        var sheetResults = new List<SheetImportResult>(sheetPlans.Count);
        var errors = new List<ImportRowError>();
        var savedCount = 0;

        logger.LogInformation(
            EngineLogMessages.ImportStarted,
            work.FileName,
            work.Template.StartRow,
            ImportRequestValidator.ResolveHeaderRow(work.Template),
            string.Join(',', sheetPlans.Select(p => p.TargetSheetName)),
            sheetPlans.Sum(p => p.Plan.Mappings.Count));

        try
        {
            foreach (var target in sheetPlans)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // MiniExcel đã đọc stream khi GetSheetNames / Query — rewind trước mỗi sheet
                SheetNameResolver.ResetStream(work.FileStream);

                var (sheetResult, sheetSaved) = await ImportOneSheetAsync(
                    work,
                    target.Plan,
                    target.TargetSheetName,
                    stopOnError,
                    errors,
                    cancellationToken);

                sheetResults.Add(sheetResult);
                savedCount += sheetSaved;

                if (stopOnError && errors.Count > 0)
                    break;
            }

            // Workbook không có dòng data sau StartRow → coi là lỗi parse (không phải success rỗng)
            if (sheetResults.Sum(s => s.ProcessedRowCount) == 0)
                throw new ExcelParsingException(EngineMessages.NoDataRowsFound(work.Template.StartRow));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ExcelEngineException ex)
        {
            logger.LogWarning(ex, EngineLogMessages.ImportFailed, work.FileName, ex.ErrorCode);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, EngineLogMessages.ImportFailedUnexpectedly, work.FileName);
            throw new ExcelParsingException(EngineMessages.FailedToParseExcel, exception);
        }

        var result = ImportResult.From(sheetResults, errors, savedCount);
        stopwatch.Stop();

        logger.LogInformation(
            EngineLogMessages.ImportFinished,
            work.FileName,
            string.Join(',', sheetResults.Select(s => s.SheetName)),
            result.ProcessedRowCount,
            result.ValidRowCount,
            result.FailedRowCount);

        logger.LogInformation(
            EngineLogMessages.ImportDuration,
            stopwatch.ElapsedMilliseconds,
            result.SavedCount,
            sheetResults.Any(s => s.RowsOmitted));

        return result;
    }

    /// <summary>
    /// Stream một sheet: QueryAsync → map/validate từng row → flush persist batch → SheetImportResult.
    /// </summary>
    /// <param name="work">File stream + template code (dùng khi gọi persister).</param>
    /// <param name="plan">Layout của sheet (mappings, header/start/rowCount).</param>
    /// <param name="sheetName">Tên sheet đã resolve trong workbook.</param>
    /// <param name="stopOnError">true = break sớm khi có lỗi validate/structure.</param>
    /// <param name="errors">Danh sách lỗi tích lũy toàn import (shared giữa các sheet).</param>
    private async Task<(SheetImportResult Result, int SavedCount)> ImportOneSheetAsync(
        ImportWork work,
        SheetTemplate plan,
        string sheetName,
        bool stopOnError,
        List<ImportRowError> errors,
        CancellationToken cancellationToken)
    {
        var headerRow = plan.ResolveHeaderRow();
        // Cap = min(template.RowCount, MaxRowCount); không set template → chỉ MaxRowCount
        var maxRows = ResolveMaxRows(plan.RowCount);
        var mappings = plan.Mappings as ColumnMapping[] ?? plan.Mappings.ToArray();

        logger.LogInformation(
            EngineLogMessages.ImportSheetStarted,
            sheetName,
            plan.StartRow,
            headerRow,
            mappings.Length);

        // Batch persist + optional buffer rows trả về response (IncludeRowsInResponse)
        var persistEnabled = work.Phase != ImportExecutionPhase.ValidateOnly;
        var batchBuffer = new ImportPersistBatchBuffer(
            dataPersister,
            _options.PersistBatchSize,
            _options.IncludeRowsInResponse,
            _options.IncludeRowsMaxRowCount,
            persistEnabled,
            logger);

        var processedRowCount = 0;
        var savedCount = 0;
        // MiniExcel với useHeaderRow: dòng đầu query = hàng ngay sau header; theo dõi số dòng Excel 1-based
        var excelRowNumber = headerRow + 1;
        var columnLimitChecked = false;
        // ColumnName mapping → key header thực tế trong dictionary MiniExcel (giữ casing file)
        Dictionary<string, string>? columnKeyMap = null;
        // Unique in-file: FieldName → set giá trị đã thấy
        var seenUnique = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var mappingList = (IReadOnlyList<ColumnMapping>)mappings;
        var sheetErrorRows = new HashSet<int>();

        // Context tối thiểu cho IImportDataPersister (consumer biết template nào đang import)
        var templateDef = new TemplateDefinition
        {
            Code = work.TemplateCode ?? string.Empty,
            EntityKey = work.EntityKey,
            Name = work.TemplateCode ?? work.FileName,
            FileName = work.FileName,
            Mappings = plan.Mappings,
            Sheets = work.Template.Sheets,
            HeaderRow = headerRow,
            StartRow = plan.StartRow,
            RowCount = plan.RowCount,
            SheetName = sheetName
        };

        // Streaming — không materialize toàn bộ sheet
        // CSV has no zip magic bytes; MiniExcel requires explicit ExcelType from file extension.
        var rows = await MiniExcelLibs.MiniExcel.QueryAsync(
            work.FileStream,
            useHeaderRow: true,
            sheetName: sheetName,
            excelType: FileNameHelper.ResolveExcelType(work.FileName),
            startCell: $"A{headerRow}",
            cancellationToken: cancellationToken);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Bỏ qua khoảng trống giữa header và StartRow (nếu StartRow > headerRow+1)
            if (excelRowNumber < plan.StartRow)
            {
                excelRowNumber++;
                continue;
            }

            if (row is not IDictionary<string, object> source)
            {
                if (!TryAcceptRow(ref processedRowCount, maxRows, plan.RowCount))
                    break;

                errors.Add(new ImportRowError(
                    excelRowNumber,
                    EngineMessages.InvalidRowStructure,
                    ErrorCode: ImportErrorCodes.InvalidRowStructure,
                    SheetName: sheetName));
                sheetErrorRows.Add(excelRowNumber);

                if (stopOnError)
                    break;

                excelRowNumber++;
                continue;
            }

            // Chỉ check số cột lần đầu gặp dictionary hợp lệ
            if (!columnLimitChecked)
            {
                columnLimitChecked = true;
                if (source.Count > _options.MaxColumnCount)
                {
                    throw new InvalidTemplateException(
                        EngineMessages.ColumnLimitExceeded(source.Count, _options.MaxColumnCount));
                }
            }

            // Build map header 1 lần / sheet; Required missing header → fail-fast InvalidTemplate
            if (mappings.Length > 0 && columnKeyMap is null)
            {
                columnKeyMap = ExcelRowMapper.BuildColumnKeyMap(source, mappings);
                ExcelRowMapper.EnsureMappedColumnsExist(mappings, columnKeyMap);
            }

            // Hàng trống hoàn toàn — bỏ qua, không tính processed
            if (ExcelRowMapper.IsEmptyRow(source))
            {
                excelRowNumber++;
                continue;
            }

            if (!TryAcceptRow(ref processedRowCount, maxRows, plan.RowCount))
                break;

            if (mappings.Length == 0)
            {
                // Không mapping → lấy mọi cột (hiếm; validator thường bắt buộc ≥1 mapping)
                savedCount += await batchBuffer.AcceptAsync(
                    ExcelRowMapper.MapAllColumns(source),
                    templateDef,
                    errors,
                    cancellationToken);
            }
            else
            {
                // Map ColumnName → FieldName; validate required/unique/regex/min/max
                var mapped = ExcelRowMapper.MapRow(source, mappings, columnKeyMap!);
                var ctx = new RowValidationContext
                {
                    ExcelRowNumber = excelRowNumber,
                    SheetName = sheetName,
                    Mappings = mappingList,
                    SeenUniqueValues = seenUnique
                };

                var fieldErrors = await rowValidator.ValidateAsync(mapped, ctx, cancellationToken);
                if (fieldErrors.Count > 0)
                {
                    errors.AddRange(fieldErrors);
                    sheetErrorRows.Add(excelRowNumber);
                    foreach (var err in fieldErrors)
                    {
                        logger.LogWarning(
                            EngineLogMessages.ImportRowValidationFailed,
                            err.SheetName,
                            err.RowNumber,
                            err.ColumnName,
                            err.FieldName,
                            err.ErrorCode,
                            err.Message);
                    }

                    if (stopOnError)
                        break;
                }
                else
                {
                    // Row hợp lệ → vào batch; đủ PersistBatchSize thì SaveAsync + clear
                    savedCount += await batchBuffer.AcceptAsync(
                        mapped, templateDef, errors, cancellationToken);
                }
            }

            excelRowNumber++;
        }

        // Flush phần dư cuối sheet (batch chưa đầy)
        savedCount += await batchBuffer.FlushRemainingAsync(templateDef, errors, cancellationToken);
        var payloadRows = batchBuffer.BuildResponseRows();

        logger.LogInformation(
            EngineLogMessages.ImportSheetFinished,
            sheetName,
            processedRowCount,
            batchBuffer.ValidRowCount,
            sheetErrorRows.Count,
            savedCount);

        return (
            new SheetImportResult
            {
                SheetName = sheetName,
                Rows = payloadRows,
                ProcessedRowCount = processedRowCount,
                FailedRowCount = sheetErrorRows.Count,
                ValidRowCount = batchBuffer.ValidRowCount,
                RowsOmitted = batchBuffer.RowsOmittedInResponse
            },
            savedCount);
    }

    /// <summary>
    /// Giới hạn số row xử lý: <c>min(template.RowCount, MaxRowCount)</c> hoặc chỉ <c>MaxRowCount</c>.
    /// </summary>
    private int ResolveMaxRows(int? templateRowCount) =>
        templateRowCount is { } requested
            ? Math.Min(requested, _options.MaxRowCount)
            : _options.MaxRowCount;

    /// <summary>
    /// Tăng <paramref name="processedRowCount"/> nếu còn slot; chạm cap template → stop êm;
    /// chạm <c>MaxRowCount</c> toàn cục (không có template RowCount) → <see cref="RowLimitExceededException"/>.
    /// </summary>
    private bool TryAcceptRow(ref int processedRowCount, int maxRows, int? templateRowCount)
    {
        if (processedRowCount >= maxRows)
        {
            if (templateRowCount is null)
                throw new RowLimitExceededException(processedRowCount + 1, _options.MaxRowCount);
            return false;
        }

        processedRowCount++;
        return true;
    }
}
