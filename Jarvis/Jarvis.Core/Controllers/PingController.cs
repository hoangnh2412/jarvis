using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Route("ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        public PingController()
        {
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}