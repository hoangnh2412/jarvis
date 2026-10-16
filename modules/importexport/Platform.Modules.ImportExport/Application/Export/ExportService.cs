using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Common.Execution;
using Platform.Modules.ImportExport.Application.Export.Models;
using Platform.Modules.ImportExport.Application.Templates;
using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Export;

/// <summary>
/// Façade export công khai (DLL / API gọi qua <see cref="IExportService"/>).
/// </summary>
public sealed class ExportService(
    ITemplateService templateService,
    IExportProcessor exportProcessor,
    IExecutionExecutor executionExecutor,
    ILogger<ExportService> logger) : IExportService
{
    public async Task<ExportResult> ExportAsync<T>(
        ExportRequest<T> request,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Data);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TemplateCode);

        var templateCode = request.TemplateCode.Trim();
        var stopwatch = Stopwatch.StartNew();

        var template = await templateService.GetAsync(templateCode, cancellationToken);

        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? template.FileName
            : request.FileName.Trim();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Export",
            [LogPropertyNames.TemplateCode] = templateCode,
            [LogPropertyNames.FileName] = fileName,
            [LogPropertyNames.Phase] = "Export"
        });

        logger.LogInformation(
            EngineLogMessages.ExportRequestStarted,
            templateCode,
            fileName);

        var work = new ExportWork<T>
        {
            Data = request.Data,
            Mappings = template.Mappings as IReadOnlyCollection<ColumnMapping> ?? template.Mappings.ToList(),
            FileName = fileName,
            SheetName = template.SheetName,
            TemplateContent = template.Content.Length == 0 ? null : template.Content,
            HeaderRow = template.HeaderRow,
            StartRow = template.StartRow,
            RowCount = template.RowCount
        };

        var result = await executionExecutor.ExecuteAsync(
            request.ExecutionMode,
            token => exportProcessor.ExportAsync(work, token),
            cancellationToken);

        stopwatch.Stop();
        logger.LogInformation(
            EngineLogMessages.ExportRequestFinished,
            templateCode,
            result.FileName,
            result.Content.Length,
            stopwatch.ElapsedMilliseconds);

        return result;
    }
}
