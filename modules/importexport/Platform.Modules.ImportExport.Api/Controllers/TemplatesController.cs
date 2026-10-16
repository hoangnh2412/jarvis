using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.API.Constants;
using Platform.Modules.ImportExport.API.Helpers;
using Platform.Modules.ImportExport.API.Mapping;
using Platform.Modules.ImportExport.API.Models;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.API.Controllers;

/// <summary>
/// HTTP API chuẩn CRUD template Excel.
/// Đăng ký qua <c>AddCoreImport(...).UseHttpApi()</c> — không kèm Authorize (host tự bảo vệ nếu cần).
/// </summary>
[ApiVersionNeutral]
[ApiController]
[Route("api/v{version:apiVersion}/templates")]
public sealed class TemplatesController(
    ITemplateService templateService,
    ILogger<TemplatesController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes(ConfigurationKeys.MultipartFormData)]
    [ProducesResponseType<TemplateApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateApiResponse>> Create(
        [FromForm] CreateTemplateForm form,
        CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.FileRequired);

        using var scope = BeginTemplateScope("Create", form.Code);
        logger.LogInformation(EngineLogMessages.ApiTemplateMutation, "Create", form.Code);

        var layout = ColumnMappingParser.ParseLayout(Request.Form[ImportExportFormFieldNames.Mappings]);
        var created = await templateService.CreateAsync(new CreateTemplateRequest
        {
            Code = form.Code,
            Name = form.Name,
            FileName = form.File.FileName,
            Content = await ReadBytesAsync(form.File, cancellationToken),
            Mappings = layout.Mappings,
            Sheets = layout.Sheets,
            SheetName = layout.SheetName,
            HeaderRow = form.HeaderRow,
            StartRow = form.StartRow,
            RowCount = form.RowCount
        }, cancellationToken);

        return Ok(TemplateApiMapper.ToApi(created));
    }

    [HttpPatch("{code}")]
    [Consumes("application/json")]
    [ProducesResponseType<TemplateApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateApiResponse>> UpdateMetadata(
        string code,
        [FromBody] UpdateTemplateMetadataForm form,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Name))
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.TemplateNameRequired);

        using var scope = BeginTemplateScope("UpdateMetadata", code);
        logger.LogInformation(EngineLogMessages.ApiTemplateMutation, "UpdateMetadata", code);

        var updated = await templateService.UpdateMetadataAsync(code, new UpdateTemplateMetadataRequest
        {
            Name = form.Name
        }, cancellationToken);

        return Ok(TemplateApiMapper.ToApi(updated));
    }

    [HttpPut("{code}/configuration")]
    [Consumes(ConfigurationKeys.MultipartFormData)]
    [ProducesResponseType<TemplateApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateApiResponse>> UpdateConfiguration(
        string code,
        [FromForm] UpdateTemplateConfigurationForm form,
        CancellationToken cancellationToken)
    {
        using var scope = BeginTemplateScope("UpdateConfiguration", code);
        logger.LogInformation(EngineLogMessages.ApiTemplateMutation, "UpdateConfiguration", code);
        return Ok(TemplateApiMapper.ToApi(await UpdateConfigurationCoreAsync(code, form, cancellationToken)));
    }

    /// <summary>Deprecated — use PUT <c>{code}/configuration</c>.</summary>
    [HttpPut("{code}")]
    [Consumes(ConfigurationKeys.MultipartFormData)]
    [ProducesResponseType<TemplateApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateApiResponse>> Update(
        string code,
        [FromForm] UpdateTemplateConfigurationForm form,
        CancellationToken cancellationToken)
    {
        using var scope = BeginTemplateScope("Update", code);
        logger.LogInformation(EngineLogMessages.ApiTemplateMutation, "Update", code);
        return Ok(TemplateApiMapper.ToApi(await UpdateConfigurationCoreAsync(code, form, cancellationToken)));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TemplateApiResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TemplateApiResponse>>> List(
        CancellationToken cancellationToken)
    {
        var items = await templateService.ListAsync(cancellationToken);
        IReadOnlyList<TemplateApiResponse> response = items
            .Select(TemplateApiMapper.ToApi)
            .ToList();
        return Ok(response);
    }

    [HttpGet("{code}")]
    [ProducesResponseType<TemplateApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateApiResponse>> Get(
        string code,
        CancellationToken cancellationToken)
    {
        var template = await templateService.GetAsync(code, cancellationToken);
        return Ok(TemplateApiMapper.ToApi(template));
    }

    [HttpGet("{code}/downloadFile")]
    [ProducesResponseType<FileDownloadResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FileDownloadResponse>> DownloadFile(
        string code,
        CancellationToken cancellationToken)
    {
        var template = await templateService.GetAsync(code, cancellationToken);
        return Ok(new FileDownloadResponse
        {
            FileName = template.FileName,
            ContentType = ExportResult.ContentType,
            ContentBase64 = Convert.ToBase64String(template.Content)
        });
    }

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        string code,
        CancellationToken cancellationToken)
    {
        using var scope = BeginTemplateScope("Delete", code);
        logger.LogInformation(EngineLogMessages.ApiTemplateMutation, "Delete", code);
        await templateService.DeleteAsync(code, cancellationToken);
        return Ok();
    }

    private IDisposable? BeginTemplateScope(string operation, string? code) =>
        logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = operation,
            [LogPropertyNames.TemplateCode] = code?.Trim(),
            [LogPropertyNames.FileName] = null,
            [LogPropertyNames.Phase] = "Template"
        });

    private async Task<TemplateDetail> UpdateConfigurationCoreAsync(
        string code,
        UpdateTemplateConfigurationForm form,
        CancellationToken cancellationToken)
    {
        byte[]? content = null;
        string? fileName = null;
        if (form.File is not null)
        {
            if (form.File.Length == 0)
                throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.FileRequired);

            content = await ReadBytesAsync(form.File, cancellationToken);
            fileName = form.File.FileName;
        }

        IReadOnlyList<ColumnMapping>? mappings = null;
        IReadOnlyList<SheetTemplate>? sheets = null;
        string? sheetName = null;
        if (Request.Form.ContainsKey(ImportExportFormFieldNames.Mappings) &&
            Request.Form[ImportExportFormFieldNames.Mappings].Any(v => !string.IsNullOrWhiteSpace(v)))
        {
            var layout = ColumnMappingParser.ParseLayout(Request.Form[ImportExportFormFieldNames.Mappings]);
            mappings = layout.Mappings;
            sheets = layout.Sheets;
            sheetName = layout.SheetName;
        }

        return await templateService.UpdateConfigurationAsync(code, new UpdateTemplateConfigurationRequest
        {
            FileName = fileName,
            Content = content,
            Mappings = mappings,
            Sheets = sheets,
            SheetName = sheetName,
            HeaderRow = form.HeaderRow,
            StartRow = form.StartRow,
            RowCount = form.RowCount
        }, cancellationToken);
    }

    private static async Task<byte[]> ReadBytesAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream(capacity: (int)Math.Min(file.Length, int.MaxValue));
        await stream.CopyToAsync(memory, cancellationToken);
        return memory.ToArray();
    }
}
