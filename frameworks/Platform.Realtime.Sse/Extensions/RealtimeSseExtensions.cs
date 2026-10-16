using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Contracts;
using Platform.Realtime.Hosting;
using Platform.Realtime.Services;
using Platform.Realtime.Identity;
using Platform.Realtime.Sse.Services;

namespace Platform.Realtime.Sse.Extensions;

public static class RealtimeSseExtensions
{
    /// <summary>
    /// Đăng ký SSE transport (registry + notifier). Bật Redis fan-out nếu cấu hình bật.
    /// </summary>
    public static RealtimeBuilder UseSse(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterSseServices(builder);

        return builder;
    }

    /// <summary>
    /// Đăng ký SSE + shared identity resolver cho user/tenant types của Host.
    /// </summary>
    public static RealtimeBuilder UseSse<TUser, TTenant>(this RealtimeBuilder builder)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterSseServices(builder);
        builder.HostBuilder.Services.TryAddScoped<RealtimeConnectionIdentityResolver<TUser, TTenant>>();
        return builder;
    }

    private static void RegisterSseServices(RealtimeBuilder builder)
    {
        if (builder.HostBuilder.Services.Any(d => d.ServiceType == typeof(SseConnectionRegistry)))
            return;

        builder.HostBuilder.Services.TryAddSingleton<SseConnectionRegistry>();
        builder.HostBuilder.Services.TryAddSingleton<SseRealtimeNotifier>();
        builder.HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IRealtimeTransport, SseRealtimeNotifier>());

        if (builder.HostBuilder.Services.All(d => d.ServiceType != typeof(SseRedisFanOutSubscriber)))
            builder.HostBuilder.Services.AddHostedService<SseRedisFanOutSubscriber>();

        builder.HostBuilder.Services.TryAddSingleton<IRealtimeRedisMultiplexer, LazyRealtimeRedisMultiplexer>();
    }
}
