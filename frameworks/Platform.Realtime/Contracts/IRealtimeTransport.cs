namespace Platform.Realtime.Contracts;

internal interface IRealtimeTransport
{

    Task PublishToUserAsync(
        Guid userId,
        object message,
        CancellationToken cancellationToken = default);

    Task PublishToUsersAsync(
        IEnumerable<Guid> userIds,
        object message,
        CancellationToken cancellationToken = default);

    Task PublishToGroupAsync(
        string groupName,
        object message,
        CancellationToken cancellationToken = default);
}
