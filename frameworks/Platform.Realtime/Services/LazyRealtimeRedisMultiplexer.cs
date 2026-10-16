using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Platform.Realtime.Configuration;

namespace Platform.Realtime.Services;

internal sealed class LazyRealtimeRedisMultiplexer : IRealtimeRedisMultiplexer
{
    private readonly Lazy<IConnectionMultiplexer> _lazy;

    public LazyRealtimeRedisMultiplexer(IOptions<PlatformRealtimeOptions> options)
    {
        _lazy = new Lazy<IConnectionMultiplexer>(() =>
        {
            var config = options.Value.Redis.Configuration;
            if (string.IsNullOrWhiteSpace(config))
                throw new InvalidOperationException("Realtime:Redis:Configuration is required.");
            return ConnectionMultiplexer.Connect(config);
        });
    }

    public IConnectionMultiplexer Multiplexer => _lazy.Value;
}