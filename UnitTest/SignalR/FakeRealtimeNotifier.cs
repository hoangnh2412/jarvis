using System.Collections.Concurrent;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Realtime.Contracts;

namespace UnitTest.SignalR;

internal sealed class FakeRealtimeNotifier(FakeNotificationStore store) : IRealtimeNotifier
{
    public int PublishCount { get; private set; }
    public bool SawSaveBeforePublish { get; private set; }
    public bool ThrowOnPublish { get; init; }
    public SignalRNotificationMessage? LastPublishedMessage { get; private set; }
    public ConcurrentBag<Guid> PublishedUserIds { get; } = [];

    public Task PublishToUserAsync(Guid userId, object message, CancellationToken cancellationToken = default)
    {
        SawSaveBeforePublish = store.SaveCount > 0;
        PublishCount++;
        LastPublishedMessage = message as SignalRNotificationMessage;
        PublishedUserIds.Add(userId);

        if (ThrowOnPublish)
            throw new InvalidOperationException("SignalR unavailable");

        return Task.CompletedTask;
    }

    public Task PublishToUsersAsync(IEnumerable<Guid> userIds, object message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishToGroupAsync(string groupName, object message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
