namespace Module.Notifications.Models;

/// <summary>
/// Response để lấy số lượng thông báo chưa đọc
/// </summary>
/// <param name="UnreadCount">Số lượng thông báo chưa đọc</param>
public sealed record UnreadCountResponse(long UnreadCount);