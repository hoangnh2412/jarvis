using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Jarvis.Authentication;
using Jarvis.DDD.Domain.Services;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Realtime.Configuration;
using Jarvis.Realtime.SignalR.Groups;
using Jarvis.Realtime.SignalR.Hubs;
using Jarvis.Realtime.SignalR.Services;

namespace UnitTest.Realtime;

public class HubRealtimeNotifierTests
{
    private sealed class TestTenantInfo : ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }

    [Fact]
    public async Task PublishToUserAsync_Sends_To_User_Group_With_ClientMethod()
    {
        var proxy = new RecordingClientProxy();
        var hubContext = new FakeHubContext(proxy);
        var options = Options.Create(new JarvisRealtimeOptions());
        var sut = new HubRealtimeNotifier<CurrentUserInfo, TestTenantInfo>(hubContext, options);
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await sut.PublishToUserAsync(userId, new SignalRNotificationMessage
        {
            Type = "demo",
            Title = "Hello"
        });

        Assert.Equal(RealtimeGroupNames.ForUser(userId), hubContext.LastGroup);
        Assert.Equal(options.Value.ClientMethodName, proxy.LastMethod);
        var msg = Assert.IsType<SignalRNotificationMessage>(proxy.LastArgs![0]);
        Assert.Equal("Hello", msg.Title);
        Assert.Equal("demo", msg.Type);
    }

    [Fact]
    public async Task PublishToUserAsync_Null_Message_Throws()
    {
        var sut = new HubRealtimeNotifier<CurrentUserInfo, TestTenantInfo>(
            new FakeHubContext(new RecordingClientProxy()),
            Options.Create(new JarvisRealtimeOptions()));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.PublishToUserAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task PublishToGroupAsync_Sends_To_Named_Group()
    {
        var proxy = new RecordingClientProxy();
        var hubContext = new FakeHubContext(proxy);
        var options = Options.Create(new JarvisRealtimeOptions());
        var sut = new HubRealtimeNotifier<CurrentUserInfo, TestTenantInfo>(hubContext, options);
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var group = RealtimeGroupNames.ForTenant(tenantId);

        await sut.PublishToGroupAsync(group, new SignalRNotificationMessage
        {
            Type = "broadcast",
            Title = "All"
        });

        Assert.Equal(group, hubContext.LastGroup);
        Assert.Equal(options.Value.ClientMethodName, proxy.LastMethod);
    }

    [Fact]
    public async Task PublishToGroupAsync_Blank_Group_Throws()
    {
        var sut = new HubRealtimeNotifier<CurrentUserInfo, TestTenantInfo>(
            new FakeHubContext(new RecordingClientProxy()),
            Options.Create(new JarvisRealtimeOptions()));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.PublishToGroupAsync(" ", new { }));
    }

    private sealed class RecordingClientProxy : IClientProxy
    {
        public string? LastMethod { get; private set; }
        public object?[]? LastArgs { get; private set; }

        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            LastMethod = method;
            LastArgs = args;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHubContext(IClientProxy proxy)
        : IHubContext<NotificationHub<CurrentUserInfo, TestTenantInfo>>
    {
        public string? LastGroup { get; private set; }
        public IHubClients Clients => new FakeClients(this, proxy);
        public IGroupManager Groups => throw new NotSupportedException();

        private sealed class FakeClients(FakeHubContext owner, IClientProxy proxy) : IHubClients
        {
            public IClientProxy All => throw new NotSupportedException();
            public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
            public IClientProxy Client(string connectionId) => throw new NotSupportedException();
            public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
            public IClientProxy Group(string groupName)
            {
                owner.LastGroup = groupName;
                return proxy;
            }
            public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
            public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
            public IClientProxy User(string userId) => throw new NotSupportedException();
            public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
        }
    }
}
