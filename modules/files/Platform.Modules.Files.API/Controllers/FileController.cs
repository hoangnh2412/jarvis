using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Files;
using Platform.Modules.Files.API.Mapping;
using Platform.Modules.Files.API.Models;
using Platform.Modules.Files.Services;
using Platform.Modules.Files.Validation;

namespace Platform.Modules.Files.API.Controllers;

/// <summary>
/// Unified file management HTTP API at <c>api/v{version}/files</c>.
/// Uses <see cref="IFileManager"/> — blob provider is resolved by host configuration.
/// Empty <c>{bucket}</c> = provider storage root (Synology RootPath, FileSystem drive, …).
/// </summary>
[ApiController]
[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/files")]
public sealed class FileController(IFileManager fileManager) : ControllerBase
{
    private static string Bucket(string? bucket) => bucket ?? string.Empty;

    [HttpPost]
    [HttpPost("{bucket}")]
    public async Task<IActionResult> CreateBucketAsync(
        string? bucket,
        [FromQuery] string? path,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateCreateBucket(resolved, path);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        try
        {
            await fileManager.CreateBucketAsync(resolved, path, cancellationToken);
            return NoContent();
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
        }
    }

    [HttpPost("upload")]
    [HttpPost("{bucket}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAsync(
        string? bucket,
        [FromQuery] string? fileName,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateUpload(resolved, fileName, file?.Length);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        await using var stream = file!.OpenReadStream();
        await fileManager.UploadStreamAsync(
            resolved,
            fileName!,
            stream,
            contentLength: file.Length > 0 ? file.Length : null,
            cancellationToken);
        return Ok(new { bucket = resolved, fileName, file.Length });
    }

    [HttpGet("download")]
    [HttpGet("{bucket}/download")]
    public async Task<IActionResult> DownloadAsync(
        string? bucket,
        [FromQuery] string fileName,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateDownload(resolved, fileName);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        var readResult = await fileManager.OpenReadAsync(resolved, fileName, cancellationToken);
        return File(
            readResult.Content,
            readResult.ContentType ?? "application/octet-stream",
            Path.GetFileName(fileName),
            enableRangeProcessing: false);
    }

    [HttpDelete]
    [HttpDelete("{bucket}")]
    public async Task<IActionResult> DeleteAsync(
        string? bucket,
        [FromQuery] string? fileName,
        [FromQuery] string? prefix,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateDelete(resolved, fileName, prefix);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            await fileManager.DeleteFileAsync(resolved, fileName, cancellationToken);
            return NoContent();
        }

        await fileManager.DeleteBucketAsync(resolved, prefix, cancellationToken);
        return NoContent();
    }

    [HttpPost("delete-batch")]
    [HttpPost("{bucket}/delete-batch")]
    public async Task<IActionResult> DeleteBatchAsync(
        string? bucket,
        [FromBody] string[] fileNames,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateDeleteBatch(resolved, fileNames);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        await fileManager.DeleteFilesAsync(resolved, fileNames, cancellationToken);
        return NoContent();
    }

    [HttpGet("view")]
    [HttpGet("{bucket}/view")]
    public async Task<IActionResult> ViewAsync(
        string? bucket,
        [FromQuery] string fileName,
        [FromQuery] int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateView(resolved, fileName);
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        var url = await fileManager.ViewAsync(resolved, fileName, expireTime, cancellationToken);
        return Ok(new { url });
    }

    [HttpGet]
    [HttpGet("{bucket}")]
    public async Task<IActionResult> ListAsync(
        string? bucket,
        [FromQuery] string? prefix,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var names = await fileManager.ListAsync(resolved, prefix, cancellationToken);
        return Ok(names);
    }

    [HttpPatch("rename")]
    [HttpPatch("{bucket}/rename")]
    public async Task<IActionResult> RenameAsync(
        string? bucket,
        [FromBody] RenameFileRequest body,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateRename(FileHttpRequestMapping.ToInput(body));
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        try
        {
            await fileManager.RenameAsync(resolved, body.Path, body.NewName, cancellationToken);
            return string.IsNullOrEmpty(body.Path)
                ? Ok(new { bucket = body.NewName })
                : NoContent();
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
        }
    }

    [HttpPost("move")]
    [HttpPost("{bucket}/move")]
    public async Task<IActionResult> MoveAsync(
        string? bucket,
        [FromBody] MoveFileRequest body,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateMove(FileHttpRequestMapping.ToInput(body));
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        try
        {
            await fileManager.MoveAsync(resolved, body.SourcePath, body.DestinationFolder, cancellationToken);
            return NoContent();
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
        }
    }

    [HttpPost("move-batch")]
    [HttpPost("{bucket}/move-batch")]
    public async Task<IActionResult> MoveBatchAsync(
        string? bucket,
        [FromBody] MoveBatchFileRequest body,
        CancellationToken cancellationToken)
    {
        var resolved = Bucket(bucket);
        var validation = FilePathValidator.ValidateMoveBatch(FileHttpRequestMapping.ToInput(body));
        if (!validation.IsValid)
            return FileApiResponses.ValidationError(validation);

        try
        {
            await fileManager.MoveFilesAsync(
                resolved,
                body.DestinationFolder,
                body.SourcePaths,
                cancellationToken);
            return NoContent();
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
        }
    }
}
