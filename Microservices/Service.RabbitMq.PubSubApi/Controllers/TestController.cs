// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;

// namespace Service.RabbitMq.PubSubApi.Controllers
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class TestController : ControllerBase
//     {
//         private readonly ITestService _testService;
//         private readonly ILogger<TestController> _logger;

//         public TestController(
//             ITestService testService,
//             ILogger<TestController> logger)
//         {
//             _testService = testService;
//             _logger = logger;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetAsync([FromQuery]string message)
//         {
//             await _testService.PublishAsync(message);
//             return Ok();
//         }
//     }
// }
