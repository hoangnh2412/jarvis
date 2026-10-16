using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Realtime.Hosting;
using Platform.Realtime.Services;
using StackExchange.Redis;

namespace Platform.Realtime.Extensions;

public static class RealtimeRedisExtensions
{
    /// <summary>
    /// Đăng ký shared <see cref="IConnectionMultiplexer"/> từ <c>Realtime:Redis:Configuration</c>.
    /// </summary>
    public static RealtimeBuilder EnsureRealtimeRedis(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.HostBuilder.Services.Any(d => d.ServiceType == typeof(IRealtimeRedisMultiplexer)))
            return builder;

        builder.HostBuilder.Services.TryAddSingleton<IRealtimeRedisMultiplexer, LazyRealtimeRedisMultiplexer>();

        return builder;
    }

}
