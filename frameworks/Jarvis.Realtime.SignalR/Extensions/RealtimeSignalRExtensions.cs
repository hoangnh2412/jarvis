using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Jarvis.DDD.Domain.Services;
using Jarvis.Modules.Notifications.Extensions;
using Jarvis.Realtime.Contracts;
using Jarvis.Realtime.Hosting;
using Jarvis.Realtime.SignalR.Services;

namespace Jarvis.Realtime.SignalR.Extensions;

public static class RealtimeSignalRExtensions
{
    /// <summary>
    /// Đăng ký ASP.NET Core SignalR. Bật Redis backplane nếu cấu hình bật.
    /// </summary>
    public static RealtimeBuilder UseSignalR(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SignalRServerBuilder = builder.HostBuilder.Services.AddSignalR();

        if (builder.Options.UseRedisBackplane)
            builder.UseRedisBackplane();

        return builder;
    }

    /// <summary>
    /// Đăng ký inbox <c>INotificationAppService</c> + <see cref="IRealtimeNotifier"/>.
    /// </summary>
    public static RealtimeBuilder AddNotificationAppServiceWithRealtime<TUser, TTenant>(
        this RealtimeBuilder builder)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddNotificationAppService<TUser, TTenant>();
        builder.HostBuilder.Services.TryAddSingleton<
            IRealtimeNotifier,
            HubRealtimeNotifier<TUser, TTenant>>();

        return builder;
    }
}
