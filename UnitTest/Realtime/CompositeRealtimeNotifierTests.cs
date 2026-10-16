using Platform.Realtime.Contracts;
using Platform.Realtime.Services;

namespace UnitTest.Realtime;

public class CompositeRealtimeNotifierTests
{
    [Fact]
    public async Task PublishToGroupAsync_Fans_Out_To_All_Transports()
    {
        var first = new RecordingTransport();
        var second = new RecordingTransport();
        var sut = new CompositeRealtimeNotifier([first, second]);

        await sut.PublishToGroupAsync("tenant:1", new { Title = "Hello" });

        Assert.Equal(["tenant:1"], first.Groups);
        Assert.Equal(["tenant:1"], second.Groups);
    }

    [Fact]
    public async Task PublishToUserAsync_Fans_Out_To_All_Transports()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var first = new RecordingTransport();
        var second = new RecordingTransport();
        var sut = new CompositeRealtimeNotifier([first, second]);

        await sut.PublishToUserAsync(userId, new { Title = "Hello" });

        Assert.Single(first.Users, userId);
        Assert.Single(second.Users, userId);
    }

    private sealed class RecordingTransport : IRealtimeTransport
    {
        public List<string> Groups { get; } = [];
        public List<Guid> Users { get; } = [];

        public Task PublishToUserAsync(
            Guid userId,
            object message,
            CancellationToken cancellationToken = default)
        {
            Users.Add(userId);
            return Task.CompletedTask;
        }

        public Task PublishToUsersAsync(
            IEnumerable<Guid> userIds,
            object message,
            CancellationToken cancellationToken = default)
        {
            Users.AddRange(userIds);
            return Task.CompletedTask;
        }

        public Task PublishToGroupAsync(
            string groupName,
            object message,
            CancellationToken cancellationToken = default)
        {
            Groups.Add(groupName);
            return Task.CompletedTask;
        }
    }
}
