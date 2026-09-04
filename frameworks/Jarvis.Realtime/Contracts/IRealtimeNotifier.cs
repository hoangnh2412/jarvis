namespace Jarvis.Realtime.Contracts;

/// <summary>
/// Đẩy payload tới client realtime đang kết nối. Không lưu lịch sử.
/// </summary>
public interface IRealtimeNotifier
{
    Task PublishToUserAsync(
        Guid userId,
        object message,
        CancellationToken cancellationToken = default);

    Task PublishToUsersAsync(
        IEnumerable<Guid> userIds,
        object message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi tới mọi connection đang join group (vd. <c>tenant:{tenantId}</c>).
    /// </summary>
    Task PublishToGroupAsync(
        string groupName,
        object message,
        CancellationToken cancellationToken = default);
}
