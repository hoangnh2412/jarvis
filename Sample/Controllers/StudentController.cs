using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.API.Models;
using Sample.Entities;
using Sample.ImportExport.Constants;
using Sample.Models;
using Sample.Services;

namespace Sample.Controllers;

/// <summary>
/// CRUD API for <see cref="Student"/> — sample host integration with ImportExport module.
/// Import/export resolve template codes from Platform Setting (group ImportExport) on the server.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/students")]
public sealed class StudentController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] PagedListRequest request,
        CancellationToken cancellationToken) =>
        Ok(await studentService.GetListAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await studentService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await studentService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await studentService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await studentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("import/validate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportApiResponse>> ValidateImportAsync(
        [FromForm] StudentImportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.FileRequired);

        await using var fileStream = request.File.OpenReadStream();
        return Ok(await studentService.ValidateImportAsync(fileStream, request.File.FileName, cancellationToken));
    }

    /// <summary>
    /// Import students from Excel — template code resolved from Setting on the server.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportApiResponse>> ImportAsync(
        [FromForm] StudentImportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            throw new BadRequestException(BaseErrorCode.Error, ImportExportApiMessages.FileRequired);

        await using var fileStream = request.File.OpenReadStream();
        return Ok(await studentService.ImportAsync(fileStream, request.File.FileName, cancellationToken));
    }

    /// <summary>
    /// Export students to Excel — template code resolved from Setting on the server.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType<FileDownloadResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FileDownloadResponse>> ExportAsync(
        [FromQuery] string? fileName,
        CancellationToken cancellationToken) =>
        Ok(await studentService.ExportAsync(fileName, cancellationToken));
}
