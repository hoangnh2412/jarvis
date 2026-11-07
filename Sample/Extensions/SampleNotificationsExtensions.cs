using Microsoft.Extensions.Options;
using Module.Notifications.Extensions;
using Platform.Authentication;
using Platform.Modules.Notifications.Extensions;
using Platform.Modules.Notifications.Redis.Extensions;
using Platform.Multitenancy;
using Platform.Realtime.Configuration;
using Platform.Realtime.Extensions;
using Platform.Realtime.SignalR.Extensions;
using Platform.Realtime.Sse.Extensions;

namespace Sample.Extensions;

public static class SampleNotificationsExtensions
{
    public static WebApplicationBuilder AddSampleNotifications(this WebApplicationBuilder builder)
    {
        builder.AddNotificationModule();

        var realtime = builder.AddCoreRealtime();
        if (realtime.Options.SignalR.Enabled)
            realtime.UseSignalR<CurrentUserInfo, CurrentTenantInfo>();
        if (realtime.Options.Sse.Enabled)
            realtime.UseSse<CurrentUserInfo, CurrentTenantInfo>();

        realtime.UseRedisInboxStore()
            .AddNotificationAppServiceWithRealtime<CurrentUserInfo, CurrentTenantInfo>();

        return builder;
    }

    public static WebApplication MapRealtimeEndpoints(this WebApplication app)
    {
        var realtimeOptions = app.Services.GetRequiredService<IOptions<PlatformRealtimeOptions>>().Value;
        if (realtimeOptions.SignalR.Enabled)
            app.MapRealtimeHub<CurrentUserInfo, CurrentTenantInfo>();
        if (realtimeOptions.Sse.Enabled)
            app.MapRealtimeSse<CurrentUserInfo, CurrentTenantInfo>();

        return app;
    }
}
