using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Contracts;
using Platform.Realtime.Hosting;
using Platform.Realtime.Identity;
using Platform.Realtime.SignalR.Hosting;
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
        RegisterSignalRInfrastructure(builder);
        return builder;
    }

    /// <summary>
    /// Đăng ký SignalR + hub transport cho user/tenant types của Host.
    /// </summary>
    public static RealtimeBuilder UseSignalR<TUser, TTenant>(this RealtimeBuilder builder)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterSignalRInfrastructure(builder);

        builder.HostBuilder.Services.TryAddScoped<
            RealtimeConnectionIdentityResolver<TUser, TTenant>>();
        builder.HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IRealtimeTransport, HubRealtimeNotifier<TUser, TTenant>>());

        return builder;
    }

    private static void RegisterSignalRInfrastructure(RealtimeBuilder builder)
    {
        if (builder.HostBuilder.Services.All(d => d.ServiceType != typeof(RealtimeSignalRBuilderHolder)))
        {
            var holder = new RealtimeSignalRBuilderHolder
            {
                ServerBuilder = builder.HostBuilder.Services.AddSignalR()
            };
            builder.HostBuilder.Services.AddSingleton(holder);
        }

        if (builder.Options.SignalR.UseRedisBackplane)
            builder.UseRedisBackplane();
    }
}
