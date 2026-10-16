using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Common.Execution;
using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Application.Templates;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Application.Import;

/// <summary>
/// Façade import công khai (DLL / API gọi qua <see cref="IImportService"/>).
/// </summary>
public sealed class ImportService(
    ITemplateService templateService,
    IImportProcessor importProcessor,
    IExecutionExecutor executionExecutor,
    ILogger<ImportService> logger) : IImportService
{
    public async Task<ImportResult> ImportAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TemplateCode);

        var templateCode = request.TemplateCode.Trim();
        var fileName = request.FileName;
        var phase = request.Phase.ToString();
        var entityKey = request.EntityKey?.Trim();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Import",
            [LogPropertyNames.TemplateCode] = templateCode,
            [LogPropertyNames.FileName] = fileName,
            [LogPropertyNames.Phase] = phase
        });

        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation(
            EngineLogMessages.ImportRequestStarted,
            templateCode,
            fileName,
            phase,
            entityKey);

        var template = await templateService.GetForExecutionAsync(templateCode, cancellationToken);

        var work = new ImportWork
        {
            TemplateCode = template.Code,
            FileStream = request.FileStream,
            FileName = request.FileName,
            Template = template.ToImportConfiguration(),
            TemplateFileName = template.FileName,
            Phase = request.Phase,
            EntityKey = entityKey
        };

        var result = await executionExecutor.ExecuteAsync(
            request.ExecutionMode,
            token => importProcessor.ImportAsync(work, token),
            cancellationToken);

        stopwatch.Stop();
        logger.LogInformation(
            EngineLogMessages.ImportRequestFinished,
            templateCode,
            fileName,
            result.ProcessedRowCount,
            result.ValidRowCount,
            result.FailedRowCount,
            result.SavedCount,
            stopwatch.ElapsedMilliseconds);

        return result;
    }
}
