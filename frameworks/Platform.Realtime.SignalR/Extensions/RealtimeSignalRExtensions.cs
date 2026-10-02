using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Services;
using Platform.Modules.Notifications.Extensions;
using Platform.Realtime.Contracts;
using Platform.Realtime.Hosting;
using Platform.Realtime.SignalR.Services;

namespace Platform.Realtime.SignalR.Extensions;

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
