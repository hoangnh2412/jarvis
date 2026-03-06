using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiVersion(ApiVersions.Current)]
[ApiController]
[Route("api/v{version:apiVersion}/users")]
public class User1Controller : ControllerBase
{
    [ApiVersion("1.0")]
    [HttpGet]
    public IActionResult GetV1() => Ok(new
    {
        Data = "User API v1"
    });
}