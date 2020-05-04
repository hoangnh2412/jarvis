using System;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Route("ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private readonly IServiceProvider _services;
        // private readonly ICoreUnitOfWork _uow:

        public PingController(IServiceProvider services)
        {
            _services = services;
        }

        [HttpGet]
        public IActionResult Ping()
        {
            // var random = new Random();
            // var time = random.Next(1, 3);

            // Thread.Sleep(time * 1000);
            return Ok("Pong");
        }


    }
}