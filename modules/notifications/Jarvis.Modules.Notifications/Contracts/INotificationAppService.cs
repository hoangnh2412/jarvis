namespace Jarvis.Modules.Notifications.Contracts;

/// <summary>
/// Dịch vụ ứng dụng thông báo.
/// Phạm vi tenant/user lấy nội bộ qua
/// <see cref="Jarvis.DDD.Domain.Services.ICurrentUser{TUser}"/> và
/// <see cref="Jarvis.DDD.Domain.Services.ICurrentTenant{TTenant}"/>.
/// </summary>
public interface INotificationAppService
{
    /// <summary>
    /// Lưu store rồi đẩy realtime tới <b>user đang xác thực</b>.
    /// </summary>
    Task NotifyUserAsync(
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi thông báo cho nhiều user trong <b>tenant hiện tại</b>.
    /// Dùng từ background job sau khi đã thiết lập tenant,
    /// hoặc từ scope admin đã xác thực.
    /// </summary>
    Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default);

    Task<NotificationListResult> ListAsync(
        int page,
        int size,
        NotificationListFilter readStatus = NotificationListFilter.All,
        CancellationToken cancellationToken = default);

    Task<SignalRNotificationItemDto?> GetAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<long> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    Task MarkReadAsync(
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    Task MarkUnreadAsync(
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    Task<long> MarkAllReadAsync(CancellationToken cancellationToken = default);
}
