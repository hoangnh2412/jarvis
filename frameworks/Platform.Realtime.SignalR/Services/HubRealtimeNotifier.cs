using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Configuration;
using Platform.Realtime.Contracts;
using Platform.Realtime.SignalR.Groups;
using Platform.Realtime.SignalR.Hubs;

namespace Platform.Realtime.SignalR.Services;

public sealed class HubRealtimeNotifier<TUser, TTenant>(
    IHubContext<NotificationHub<TUser, TTenant>> hubContext,
    IOptions<PlatformRealtimeOptions> options) : IRealtimeNotifier
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    public Task PublishToUserAsync(
        Guid userId,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return PublishToGroupAsync(RealtimeGroupNames.ForUser(userId), message, cancellationToken);
    }

    public async Task PublishToUsersAsync(
        IEnumerable<Guid> userIds,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentNullException.ThrowIfNull(message);
        foreach (var userId in userIds.Distinct())
            await PublishToUserAsync(userId, message, cancellationToken).ConfigureAwait(false);
    }

    public Task PublishToGroupAsync(
        string groupName,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);

        var methodName = string.IsNullOrWhiteSpace(options.Value.ClientMethodName)
            ? new PlatformRealtimeOptions().ClientMethodName
            : options.Value.ClientMethodName;

        return hubContext.Clients
            .Group(groupName)
            .SendAsync(methodName, message, cancellationToken);
    }
}
