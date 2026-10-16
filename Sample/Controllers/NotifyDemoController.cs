using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Notifications.Contracts;
using System.Text.Json;

namespace Sample.Controllers;

[ApiController]
[Route("api/notify-demo")]
[Authorize]
public class NotifyDemoController(INotificationAppService notifications) : ControllerBase
{
    public sealed record NotificationDataDto(
    string? actionUrl,
    string? actorName);

    public sealed record DemoBody(
        string? type,
        string? title,
        string? body,
        NotificationDataDto? data);

    [HttpPost("me")]
    public async Task<IActionResult> NotifyDemo([FromBody] DemoBody body, CancellationToken ct)
    {
        await notifications.NotifyUserAsync(new SignalRNotificationMessage
        {
            Type = body.type ?? "demo",
            Title = body.title ?? "Demo notification",
            Body = body.body,
            Data = body.data is null
                ? null
                : JsonSerializer.SerializeToElement(body.data),
        }, ct);

        return Ok(new { sent = true });
    }
}