using System;
using System.Linq;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Events;
using Jarvis.Core.Models.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
            // var random = new Random();
            // var time = random.Next(1, 3);

            // Thread.Sleep(time * 1000);
            return Ok("Pong");
        }


    }
}