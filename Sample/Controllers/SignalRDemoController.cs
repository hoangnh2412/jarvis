using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Modules.Notifications.Contracts;
using System.Text.Json;

namespace Sample.Controllers;

[ApiController]
[Route("api/signalr-demo")]
[Authorize]
public class SignalRDemoController(INotificationAppService notifications) : ControllerBase
{
    public sealed record NotificationDataDto(
    string? actionUrl,
    string? actorName,
    string? actorAvatarUrl,
    string? imageUrl);

    public sealed record DemoBody(
        string? type,
        string? title,
        string? body,
        NotificationDataDto? data);

    [HttpPost("me")]
    public async Task<IActionResult> NotifyMe([FromBody] DemoBody body, CancellationToken ct)
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