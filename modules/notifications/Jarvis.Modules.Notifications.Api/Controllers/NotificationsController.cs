using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Module.Notifications.Models;
using Jarvis.Modules.Notifications.Contracts;

namespace Module.Notifications.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/notifications")]
public class NotificationsController(INotificationAppService notifications) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách thông báo
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NotificationListResult>> List(
        [FromQuery] NotificationListQuery query,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var result = await notifications
            .ListAsync(query.Page, query.Size, query.ReadStatus, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    /// <summary>
    /// Lấy thông báo theo id
    /// </summary>
    [HttpGet("{notificationId:guid}")]
    public async Task<ActionResult<SignalRNotificationItemDto>> Get(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var item = await notifications.GetAsync(notificationId, cancellationToken)
            .ConfigureAwait(false);

        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountResponse>> UnreadCount(CancellationToken cancellationToken)
    {
        var count = await notifications.GetUnreadCountAsync(cancellationToken)
            .ConfigureAwait(false);

        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    [HttpPut("read")]
    public async Task<IActionResult> MarkRead(
        [FromBody] NotificationIdsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await notifications.MarkReadAsync(request.NotificationIds, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Đánh dấu thông báo chưa đọc
    /// </summary>
    [HttpPut("unread")]
    public async Task<IActionResult> MarkUnread(
        [FromBody] NotificationIdsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await notifications.MarkUnreadAsync(request.NotificationIds, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        _ = await notifications.MarkAllReadAsync(cancellationToken);
        return NoContent();
    }
}
