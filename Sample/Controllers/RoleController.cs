using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiVersionNeutral]
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