using Microsoft.AspNetCore.Mvc;

namespace {Product}.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
  [HttpGet]
  public IActionResult Get() => Ok(new { status = "ok", product = "{Product}" });
}
