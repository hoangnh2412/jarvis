using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/roles")]
public class RoleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        Data = "Role logic dùng chung cho v1 & v2"
    });
}