using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Application.Events;
using Jarvis.Infrastructure.DistributedEvent.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Sample.EventBus;

namespace Sample.Controllers;

[ApiController]
[Route("event-bus")]
public class EventBusController : ControllerBase
{
    [HttpGet("local")]
    public async Task<IActionResult> LocalAsync(
        [FromServices] IEventBus producer
    )
    {
        await producer.PublishAsync<SampleEto>(new SampleEto
        {
            Data = $"Local message: {Guid.NewGuid().ToString()}"
        });
        return Ok();
    }

    [HttpGet("distributed")]
    public async Task<IActionResult> DistributedAsync(
        [FromServices] IDistributedEventProducer producer
    )
    {
        await producer.PublishAsync<SampleEto>(new SampleEto
        {
            Data = $"Distributed message: {Guid.NewGuid().ToString()}"
        }, "sample-queue");
        return Ok();
    }
}