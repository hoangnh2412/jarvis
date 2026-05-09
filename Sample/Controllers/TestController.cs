using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/tests")]
public class TestController : ControllerBase
{
    /// <summary>Multipart upload. Do not use <c>[FromForm]</c> on <see cref="IFormFile"/> — Swashbuckle cannot generate the operation (see Swashbuckle file-upload docs).</summary>
    [HttpPost("avatar"), MapToApiVersion(1.0)]
    [Consumes("multipart/form-data")]
    public IActionResult UpdateAvatar(IFormFile file, CancellationToken cancellationToken = default)
    {
        return Ok(new
        {
            FileName = file.FileName
        });
    }
}