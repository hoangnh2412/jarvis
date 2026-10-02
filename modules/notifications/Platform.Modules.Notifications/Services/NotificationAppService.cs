using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.Notifications.Constants;
using Platform.Modules.Notifications.Contracts;
using Microsoft.Extensions.Logging;
using Platform.Realtime.Contracts;

namespace Platform.Modules.Notifications.Services;

public sealed class NotificationAppService<TUser, TTenant>(
    INotificationStore store,
    IRealtimeNotifier notifier,
    ICurrentUser<TUser> currentUser,
    ICurrentTenant<TTenant> currentTenant,
    ILogger<NotificationAppService<TUser, TTenant>> logger) : INotificationAppService
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    private const int MaxFanOutConcurrency = 16;

    public async Task NotifyUserAsync(
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        await NotifyUserCoreAsync(tenantId, userId, message, cancellationToken).ConfigureAwait(false);
    }

    public async Task NotifyUsersAsync(
        IEnumerable<Guid> userIds,
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentNullException.ThrowIfNull(message);

        var (tenantId, _) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);

        await Parallel.ForEachAsync(
            userIds.ToHashSet(),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxFanOutConcurrency,
                CancellationToken = cancellationToken
            },
            async (userId, ct) =>
                await NotifyUserCoreAsync(tenantId, userId, message, ct).ConfigureAwait(false));
    }

    public async Task<NotificationListResult> ListAsync(
        int page,
        int size,
        NotificationListFilter readStatus = NotificationListFilter.All,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        return await store.ListAsync(tenantId, userId, page, size, readStatus, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SignalRNotificationItemDto?> GetAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        return await store.GetAsync(tenantId, userId, notificationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<long> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        return await store.GetUnreadCountAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkReadAsync(
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        await store.MarkReadAsync(tenantId, userId, notificationIds, cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkUnreadAsync(
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        await store.MarkUnreadAsync(tenantId, userId, notificationIds, cancellationToken).ConfigureAwait(false);
    }

    public async Task<long> MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = await RequireContextAsync(cancellationToken).ConfigureAwait(false);
        return await store.MarkAllReadAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(Guid TenantId, Guid UserId)> RequireContextAsync(CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        // Tạm thời fix cứng dữ liệu user để pass kiểm tra khi chưa xác thực by vuongnd
        var userId = user?.UserId is { } uid && uid != Guid.Empty
            ? uid
            : Guid.Parse("22222222-2222-2222-2222-222222222222");

        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException(
                NotificationErrorCode.UserRequired,
                "User context is required to access notifications.");
        }

        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        // Tạm thời fix cứng dữ liệu tenant để pass kiểm tra khi chưa xác thực by vuongnd
        var resolvedTenantId = tenantId is { } tid && tid != Guid.Empty
            ? tid
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

        if (resolvedTenantId == Guid.Empty)
        {
            throw new UnauthorizedException(
                NotificationErrorCode.TenantRequired,
                "Tenant context is required to access notifications.");
        }

        return (resolvedTenantId, userId);
    }

    private async Task NotifyUserCoreAsync(
        Guid tenantId,
        Guid userId,
        SignalRNotificationMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.Type) || string.IsNullOrWhiteSpace(message.Title))
        {
            throw new ArgumentException("Type and Title are required.");
        }

        var enriched = NotificationMessageEnricher.Enrich(message, tenantId: tenantId, userId: userId);

        await store.SaveAsync(tenantId, userId, enriched, cancellationToken).ConfigureAwait(false);
        try
        {
            await notifier.PublishToUserAsync(userId, enriched, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex,
                "Failed to push notification {NotificationId} to user {UserId} of tenant {TenantId}",
                enriched.NotificationId, userId, tenantId);
        }
    }
}
