using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiVersion(ApiVersions.Current)]
[ApiController]
[Route("api/v{version:apiVersion}/users")]
public class User2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetV2() => Ok(new
    {
        Data = "User API v2"
    });
}