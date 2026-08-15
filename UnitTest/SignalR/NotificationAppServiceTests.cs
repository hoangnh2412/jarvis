using Microsoft.Extensions.Logging.Abstractions;
using Jarvis.Authentication;
using Jarvis.DDD.Domain.Services;
using Jarvis.DDD.Domain.Shared.ExceptionHandling;
using Jarvis.Modules.Notifications.Constants;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Services;

namespace UnitTest.SignalR;

public class NotificationAppServiceTests
{
    private static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DefaultUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task NotifyUserAsync_Saves_Then_Publishes()
    {
        var store = new FakeNotificationStore();
        var notifier = new FakeRealtimeNotifier(store);
        var sut = CreateSut(store, notifier);

        await sut.NotifyUserAsync(new SignalRNotificationMessage
        {
            Type = "demo",
            Title = "Hi"
        });

        Assert.Equal(1, store.SaveCount);
        Assert.Equal(1, notifier.PublishCount);
        Assert.True(notifier.SawSaveBeforePublish);
    }

    [Fact]
    public async Task NotifyUserAsync_Enriches_Message_Before_Save_And_Publish()
    {
        var store = new FakeNotificationStore();
        var notifier = new FakeRealtimeNotifier(store);
        var sut = CreateSut(store, notifier);

        await sut.NotifyUserAsync(new SignalRNotificationMessage
        {
            NotificationId = Guid.Empty,
            Type = "demo",
            Title = "Hi"
        });

        Assert.NotNull(store.LastSavedMessage);
        Assert.NotNull(notifier.LastPublishedMessage);
        var saved = store.LastSavedMessage!;
        var published = notifier.LastPublishedMessage!;

        Assert.NotEqual(Guid.Empty, saved.NotificationId);
        Assert.Equal(DefaultTenantId, saved.TenantId);
        Assert.Equal(DefaultUserId, saved.UserId);
        Assert.NotEqual(default, saved.CreatedAtUtc);
        Assert.Equal(saved.NotificationId, published.NotificationId);
        Assert.Equal(saved.TenantId, published.TenantId);
        Assert.Equal(saved.UserId, published.UserId);
    }

    [Fact]
    public async Task NotifyUserAsync_When_Publish_Fails_Still_Completes_After_Save()
    {
        var store = new FakeNotificationStore();
        var notifier = new FakeRealtimeNotifier(store) { ThrowOnPublish = true };
        var sut = CreateSut(store, notifier);

        await sut.NotifyUserAsync(new SignalRNotificationMessage
        {
            Type = "demo",
            Title = "Hi"
        });

        Assert.Equal(1, store.SaveCount);
        Assert.Equal(1, notifier.PublishCount);
    }

    [Fact]
    public async Task NotifyUsersAsync_Notifies_Each_Distinct_User()
    {
        var store = new FakeNotificationStore();
        var notifier = new FakeRealtimeNotifier(store);
        var sut = CreateSut(store, notifier);
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        await sut.NotifyUsersAsync(
            [user1, user2, user1],
            new SignalRNotificationMessage { Type = "demo", Title = "Hi" });

        Assert.Equal(2, store.SaveCount);
        Assert.Equal(2, notifier.PublishCount);
        Assert.Equal(new HashSet<Guid> { user1, user2 }, notifier.PublishedUserIds.ToHashSet());
    }

    [Fact]
    public async Task NotifyUserAsync_When_Type_Or_Title_Missing_Throws_ArgumentException()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.NotifyUserAsync(new SignalRNotificationMessage { Type = "", Title = "Hi" }));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.NotifyUserAsync(new SignalRNotificationMessage { Type = "demo", Title = "   " }));

        Assert.Equal(0, store.SaveCount);
    }

    [Fact]
    public async Task NotifyUserAsync_When_User_Missing_Throws_Unauthorized()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(
            store,
            new FakeRealtimeNotifier(store),
            currentUser: new FakeCurrentUser(null));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sut.NotifyUserAsync(new SignalRNotificationMessage { Type = "demo", Title = "Hi" }));

        Assert.Equal(NotificationErrorCode.UserRequired, ex.Code);
    }

    [Fact]
    public async Task NotifyUserAsync_When_Tenant_Missing_Throws_Unauthorized()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(
            store,
            new FakeRealtimeNotifier(store),
            currentTenant: new FakeCurrentTenant(null));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            sut.NotifyUserAsync(new SignalRNotificationMessage { Type = "demo", Title = "Hi" }));

        Assert.Equal(NotificationErrorCode.TenantRequired, ex.Code);
    }

    [Fact]
    public async Task ListAsync_Forwards_Filter_To_Store()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));

        await sut.ListAsync(page: 1, size: 20, readStatus: NotificationListFilter.Unread);

        Assert.Equal(NotificationListFilter.Unread, store.LastReadStatus);
        Assert.Equal(DefaultTenantId, store.LastTenantId);
        Assert.Equal(DefaultUserId, store.LastUserId);
    }

    [Theory]
    [InlineData(NotificationListFilter.All)]
    [InlineData(NotificationListFilter.Unread)]
    [InlineData(NotificationListFilter.Read)]
    public async Task ListAsync_Forwards_Each_ReadStatus(NotificationListFilter readStatus)
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));

        await sut.ListAsync(page: 2, size: 10, readStatus: readStatus);

        Assert.Equal(readStatus, store.LastReadStatus);
        Assert.Equal(2, store.LastPage);
        Assert.Equal(10, store.LastSize);
    }

    [Fact]
    public async Task ListAsync_Default_ReadStatus_Is_All()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));

        await sut.ListAsync(page: 1, size: 20);

        Assert.Equal(NotificationListFilter.All, store.LastReadStatus);
    }

    [Fact]
    public async Task MarkReadAsync_Forwards_NotificationIds_To_Store()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

        await sut.MarkReadAsync(ids);

        Assert.Equal(ids, store.LastMarkReadIds);
    }

    [Fact]
    public async Task MarkUnreadAsync_Forwards_NotificationIds_To_Store()
    {
        var store = new FakeNotificationStore();
        var sut = CreateSut(store, new FakeRealtimeNotifier(store));
        var ids = new[] { Guid.NewGuid() };

        await sut.MarkUnreadAsync(ids);

        Assert.Equal(ids, store.LastMarkUnreadIds);
    }

    private static NotificationAppService<CurrentUserInfo, TestTenantInfo> CreateSut(
        FakeNotificationStore store,
        FakeRealtimeNotifier notifier,
        ICurrentUser<CurrentUserInfo>? currentUser = null,
        ICurrentTenant<TestTenantInfo>? currentTenant = null)
    {
        return new NotificationAppService<CurrentUserInfo, TestTenantInfo>(
            store,
            notifier,
            currentUser ?? new FakeCurrentUser(DefaultUserId),
            currentTenant ?? new FakeCurrentTenant(DefaultTenantId),
            NullLogger<NotificationAppService<CurrentUserInfo, TestTenantInfo>>.Instance);
    }
}
