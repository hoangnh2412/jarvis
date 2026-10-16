using Platform.Realtime.Contracts;

namespace Platform.Realtime.Services;

internal sealed class CompositeRealtimeNotifier(IEnumerable<IRealtimeTransport> transports) : IRealtimeNotifier
{
    private readonly IRealtimeTransport[] _transports = transports.ToArray();

    public Task PublishToUserAsync(
        Guid userId,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return WhenAll(_transports, t => t.PublishToUserAsync(userId, message, cancellationToken));
    }

    public Task PublishToUsersAsync(
        IEnumerable<Guid> userIds,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentNullException.ThrowIfNull(message);
        return WhenAll(_transports, t => t.PublishToUsersAsync(userIds, message, cancellationToken));
    }

    public Task PublishToGroupAsync(
        string groupName,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);
        return WhenAll(_transports, t => t.PublishToGroupAsync(groupName, message, cancellationToken));
    }

    private static Task WhenAll(
        IReadOnlyList<IRealtimeTransport> transports,
        Func<IRealtimeTransport, Task> action)
    {
        if (transports.Count == 0)
            return Task.CompletedTask;

        return Task.WhenAll(transports.Select(action));
    }
}
