using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Files;
using Platform.Modules.Files.Services;

namespace Platform.Modules.Files.API.Controllers;

/// <summary>Folder tree browse API (requires matching <see cref="Platform.Modules.Files.Storage.IFileTreeProvider"/> registration).</summary>
[ApiController]
[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/files")]
public sealed class FolderController(IFileManager fileManager) : ControllerBase
{
    [HttpGet("tree")]
    public async Task<IActionResult> GetTreeAsync(
        [FromQuery] string? bucket,
        [FromQuery] string? rootPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var tree = await fileManager.GetTreeAsync(bucket, rootPath, cancellationToken);
            return Ok(tree);
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(
                StatusCodes.Status501NotImplemented,
                new { code = FileErrorCode.TreeNotSupported, message = ex.Message });
        }
    }
}
