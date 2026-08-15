namespace Jarvis.Modules.Notifications.Contracts;

/// <summary>
/// Port lưu trữ thông báo. Implementation đăng ký bởi các package store satellite.
/// </summary>
public interface INotificationStore
{
    Task SaveAsync(
        Guid tenantId,
        Guid userId,
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default);

    Task<SignalRNotificationItemDto?> GetAsync(
        Guid tenantId,
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<NotificationListResult> ListAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int size,
        NotificationListFilter readStatus = NotificationListFilter.All,
        CancellationToken cancellationToken = default);

    Task<long> GetUnreadCountAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkReadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    Task MarkUnreadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    Task<long> MarkAllReadAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
