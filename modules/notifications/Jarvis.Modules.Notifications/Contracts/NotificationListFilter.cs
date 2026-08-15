namespace Jarvis.Modules.Notifications.Contracts;

/// <summary>
/// Enum để lọc danh sách thông báo(Tất cả: 0, Chưa đọc: 1, Đã đọc: 2)
/// </summary>
public enum NotificationListFilter
{
    All = 0,
    Unread = 1,
    Read = 2,
}
