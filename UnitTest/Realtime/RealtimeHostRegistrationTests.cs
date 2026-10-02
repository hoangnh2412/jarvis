using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platform.Authentication;
using Platform.DDD.Domain.Services;
using Platform.Modules.Notifications.Redis.Extensions;
using Platform.Realtime.Contracts;
using Platform.Realtime.Extensions;
using Platform.Realtime.SignalR.Extensions;
using Platform.Realtime.SignalR.Services;

namespace UnitTest.Realtime;

public class RealtimeHostRegistrationTests
{
    private sealed class TestTenantInfo : ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }

    [Fact]
    public void AddNotificationAppServiceWithRealtime_Registers_IRealtimeNotifier()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:Configuration"] = "127.0.0.1:6379";
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:InstanceName"] = "Notifications1_";
        builder.AddCoreRealtime()
            .UseSignalR()
            .UseRedisInboxStore()
            .AddNotificationAppServiceWithRealtime<CurrentUserInfo, TestTenantInfo>();

        using var app = builder.Build();
        var notifier = app.Services.GetRequiredService<IRealtimeNotifier>();
        Assert.IsType<HubRealtimeNotifier<CurrentUserInfo, TestTenantInfo>>(notifier);
    }

    [Fact]
    public void UseRedisInboxStore_Missing_InstanceName_Throws()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:Configuration"] = "127.0.0.1:6379";
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:InstanceName"] = "";
        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.AddCoreRealtime().UseSignalR().UseRedisInboxStore());
        Assert.Contains("InstanceName", ex.Message);
    }

    [Fact]
    public void UseRedisBackplane_Missing_Configuration_Throws()
    {
        var builder = WebApplication.CreateBuilder();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.AddCoreRealtime(o =>
            {
                o.UseRedisBackplane = true;
                o.Redis.Configuration = "";
            }).UseSignalR());
        Assert.Contains("Realtime:SignalR:Redis:Configuration", ex.Message);
    }

    [Fact]
    public void AddCoreRealtime_Alone_Does_Not_Register_Store()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddCoreRealtime().UseSignalR();
        using var app = builder.Build();
        Assert.Null(app.Services.GetService(typeof(Platform.Modules.Notifications.Contracts.INotificationStore)));
    }
}
