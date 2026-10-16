using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.API.Constants;
using Platform.Modules.ImportExport.API.Helpers;
using Platform.Modules.ImportExport.API.Models;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.API.Controllers;

/// <summary>
/// HTTP façade import/export Excel — tham số giống DLL (<see cref="ImportRequest"/> / <see cref="ExportRequest{T}"/>).
/// </summary>
[ApiVersionNeutral]
[ApiController]
[Route("api/v{version:apiVersion}/excel")]
public sealed class ExcelController(
    IImportService importService,
    IExportService exportService,
    ILogger<ExcelController> logger) : ControllerBase
{
    [HttpPost("import/validate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status422UnprocessableEntity)]
    public Task<ActionResult<ImportApiResponse>> ValidateImport(
        [FromForm] ImportApiRequest request,
        CancellationToken cancellationToken) =>
        ImportCoreAsync(request, ImportExecutionPhase.ValidateOnly, cancellationToken);

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status422UnprocessableEntity)]
    public Task<ActionResult<ImportApiResponse>> Import(
        [FromForm] ImportApiRequest request,
        CancellationToken cancellationToken) =>
        ImportCoreAsync(request, ImportExecutionPhase.Import, cancellationToken);

    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> Export(
        [FromBody] ExportApiRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateCode))
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.TemplateCodeRequired);

        var templateCode = request.TemplateCode.Trim();
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Export",
            [LogPropertyNames.TemplateCode] = templateCode,
            [LogPropertyNames.FileName] = request.FileName,
            [LogPropertyNames.Phase] = "Export"
        });

        logger.LogInformation(
            EngineLogMessages.ApiExportReceived,
            templateCode,
            request.FileName,
            request.Data.Count);

        var data = request.Data.Select(row =>
            (IReadOnlyDictionary<string, object?>)row.ToDictionary(
                x => x.Key,
                x => x.Value.ToClrValue(),
                StringComparer.OrdinalIgnoreCase));

        var result = await exportService.ExportAsync(new ExportRequest<IReadOnlyDictionary<string, object?>>
        {
            TemplateCode = request.TemplateCode,
            Data = data,
            FileName = request.FileName
        }, cancellationToken);

        return File(result.Content, ExportResult.ContentType, result.FileName);
    }

    private async Task<ActionResult<ImportApiResponse>> ImportCoreAsync(
        ImportApiRequest request,
        ImportExecutionPhase phase,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.FileRequired);

        if (string.IsNullOrWhiteSpace(request.TemplateCode))
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.TemplateCodeRequired);

        var templateCode = request.TemplateCode.Trim();
        var fileName = request.File.FileName;
        var entityKey = request.EntityKey?.Trim();
        var phaseName = phase.ToString();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = "Import",
            [LogPropertyNames.TemplateCode] = templateCode,
            [LogPropertyNames.FileName] = fileName,
            [LogPropertyNames.Phase] = phaseName
        });

        logger.LogInformation(
            EngineLogMessages.ApiImportReceived,
            templateCode,
            fileName,
            phaseName,
            entityKey);

        await using var fileStream = request.File.OpenReadStream();
        var result = await importService.ImportAsync(new ImportRequest
        {
            TemplateCode = request.TemplateCode,
            FileStream = fileStream,
            FileName = fileName,
            Phase = phase,
            EntityKey = entityKey
        }, cancellationToken);

        var payload = new ImportApiResponse(
            result.Sheets,
            result.ProcessedRowCount,
            result.FailedRowCount,
            result.SavedCount);

        if (result.FailedRowCount > 0 && result.ValidRowCount == 0)
            return UnprocessableEntity(payload);

        return Ok(payload);
    }
}
