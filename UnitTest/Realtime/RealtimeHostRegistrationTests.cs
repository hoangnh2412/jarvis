using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platform.Authentication;
using Platform.DDD.Domain.Services;
using Platform.Modules.Notifications.Extensions;
using Platform.Modules.Notifications.Redis.Extensions;
using Platform.Realtime.Contracts;
using Platform.Realtime.Extensions;
using Platform.Realtime.Services;
using Platform.Realtime.SignalR.Extensions;
using Platform.Realtime.Sse.Extensions;

namespace UnitTest.Realtime;

public class RealtimeHostRegistrationTests
{
    private sealed class TestTenantInfo : ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }

    [Fact]
    public void AddNotificationAppServiceWithRealtime_Registers_CompositeRealtimeNotifier()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:Configuration"] = "127.0.0.1:6379";
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:InstanceName"] = "Notifications1_";
        builder.AddCoreRealtime()
            .UseSignalR<CurrentUserInfo, TestTenantInfo>()
            .UseSse<CurrentUserInfo, TestTenantInfo>()
            .UseRedisInboxStore()
            .AddNotificationAppServiceWithRealtime<CurrentUserInfo, TestTenantInfo>();

        using var app = builder.Build();
        var notifier = app.Services.GetRequiredService<IRealtimeNotifier>();
        Assert.IsType<CompositeRealtimeNotifier>(notifier);
    }

    [Fact]
    public void SignalR_Disabled_Skips_Hub_Registration_But_Sse_Still_Works()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Realtime:SignalR:Enabled"] = "false";
        builder.Configuration["Realtime:Sse:Enabled"] = "true";

        builder.AddCoreRealtime()
            .UseSse<CurrentUserInfo, TestTenantInfo>()
            .AddNotificationAppServiceWithRealtime<CurrentUserInfo, TestTenantInfo>();

        using var app = builder.Build();
        Assert.IsType<CompositeRealtimeNotifier>(app.Services.GetRequiredService<IRealtimeNotifier>());
    }

    [Fact]
    public void UseRedisInboxStore_Missing_InstanceName_Throws()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:Configuration"] = "127.0.0.1:6379";
        builder.Configuration["Cache:DistributedGroups:Redis:Notifications:InstanceName"] = "";
        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.AddCoreRealtime()
                .UseSignalR<CurrentUserInfo, TestTenantInfo>()
                .UseRedisInboxStore());
        Assert.Contains("InstanceName", ex.Message);
    }

    [Fact]
    public void UseRedisBackplane_Missing_Configuration_Throws()
    {
        var builder = WebApplication.CreateBuilder();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.AddCoreRealtime(o =>
            {
                o.SignalR.UseRedisBackplane = true;
                o.Redis.Configuration = "";
            }).UseSignalR<CurrentUserInfo, TestTenantInfo>());
        Assert.Contains("Realtime:Redis:Configuration", ex.Message);
    }

    [Fact]
    public void AddCoreRealtime_Alone_Does_Not_Register_Store()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddCoreRealtime().UseSignalR<CurrentUserInfo, TestTenantInfo>();
        using var app = builder.Build();
        Assert.Null(app.Services.GetService(typeof(Platform.Modules.Notifications.Contracts.INotificationStore)));
    }
}
