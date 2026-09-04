using Jarvis.Modules.Notifications.Contracts;

namespace UnitTest.SignalR;

internal sealed class FakeNotificationStore : INotificationStore
{
    public int SaveCount { get; private set; }
    public SignalRNotificationMessage? LastSavedMessage { get; private set; }
    public NotificationListFilter? LastReadStatus { get; private set; }
    public int? LastPage { get; private set; }
    public int? LastSize { get; private set; }
    public Guid? LastTenantId { get; private set; }
    public Guid? LastUserId { get; private set; }
    public IReadOnlyCollection<Guid>? LastMarkReadIds { get; private set; }
    public IReadOnlyCollection<Guid>? LastMarkUnreadIds { get; private set; }

    public Task SaveAsync(Guid tenantId, Guid userId, SignalRNotificationMessage message, CancellationToken cancellationToken = default)
    {
        SaveCount++;
        LastSavedMessage = message;
        LastTenantId = tenantId;
        LastUserId = userId;
        return Task.CompletedTask;
    }

    public Task<SignalRNotificationItemDto?> GetAsync(Guid tenantId, Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        => Task.FromResult<SignalRNotificationItemDto?>(null);

    public Task<NotificationListResult> ListAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int size,
        NotificationListFilter readStatus = NotificationListFilter.All,
        CancellationToken cancellationToken = default)
    {
        LastReadStatus = readStatus;
        LastPage = page;
        LastSize = size;
        LastTenantId = tenantId;
        LastUserId = userId;

        return Task.FromResult(new NotificationListResult
        {
            Data = [],
            UnreadCount = 0,
            Page = page,
            Size = size,
            TotalItems = 0,
            TotalPages = 0
        });
    }

    public Task<long> GetUnreadCountAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(0L);

    public Task MarkReadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        LastMarkReadIds = notificationIds.ToArray();
        LastTenantId = tenantId;
        LastUserId = userId;
        return Task.CompletedTask;
    }

    public Task MarkUnreadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        LastMarkUnreadIds = notificationIds.ToArray();
        LastTenantId = tenantId;
        LastUserId = userId;
        return Task.CompletedTask;
    }

    public Task<long> MarkAllReadAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(0L);
}
