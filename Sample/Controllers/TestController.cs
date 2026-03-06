using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/tests")]
public class TestController : ControllerBase
{
    [HttpPost("avatar"), MapToApiVersion(1.0)]
    public IActionResult UpdateAvatar([FromForm] IFormFile file, CancellationToken cancellationToken = default)
    {
        return Ok(new
        {
            FileName = file.FileName
        });
    }
}