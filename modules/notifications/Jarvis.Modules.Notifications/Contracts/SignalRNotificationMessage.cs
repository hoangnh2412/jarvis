using System.Text.Json;
using Newtonsoft.Json;
using Jarvis.DDD.Application.Contracts.DTOs;
using Jarvis.Modules.Notifications.Serialization;

namespace Jarvis.Modules.Notifications.Contracts;

/// <summary>
/// Dữ liệu tạo thông báo: lưu store rồi đẩy sự kiện SignalR <c>notification</c>.
/// </summary>
public sealed class SignalRNotificationMessage
{
    public Guid NotificationId { get; init; } = Guid.CreateVersion7();

    /// <summary>Loại nghiệp vụ</summary>
    public required string Type { get; init; }

    public required string Title { get; init; }

    public string? Body { get; init; }

    /// <summary>Metadata JSON cho UI (actionUrl,...)</summary>
    [JsonConverter(typeof(JsonElementNewtonsoftConverter))]
    public JsonElement? Data { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? UserId { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Item thông báo trả client / hydrate từ store.
/// Trạng thái đọc = membership ZSET — <see cref="IsRead"/> tính khi List, không lưu trong JSON Redis.
/// </summary>
public sealed class SignalRNotificationItemDto
{
    public Guid NotificationId { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }

    [JsonConverter(typeof(JsonElementNewtonsoftConverter))]
    public JsonElement? Data { get; init; }

    /// <summary>
    /// Tính cho HTTP (List All theo membership unread; Unread=false; Read=true). Không lưu trong JSON Redis.
    /// </summary>
    public bool IsRead { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }
}

/// <summary>
/// Danh sách thông báo phân trang của user hiện tại.
/// </summary>
public sealed class NotificationListResult : IPagedDto<SignalRNotificationItemDto>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<SignalRNotificationItemDto> Data { get; set; } = [];
    /// <summary>Số chưa đọc của user hiện tại (không thuộc <see cref="IPagedDto{T}"/>).</summary>
    public long UnreadCount { get; set; }
}
