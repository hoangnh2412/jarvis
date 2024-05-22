using Jarvis.Application.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("exception")]
public class ExceptionController : ControllerBase
{
    [HttpGet]
    public IActionResult Sample()
    {
        throw new BusinessException(9999, "Something when wrong");
    }
}