using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace Jarvis.Persistence.Caching.Redis;

public static class RedisConnectionFactory
{
    private static ConcurrentDictionary<string, IConnectionMultiplexer> _connections = new ConcurrentDictionary<string, IConnectionMultiplexer>();

    public static IConnectionMultiplexer Connect(RedisCacheOptions options, CancellationToken token = default(CancellationToken))
    {
        return _connections.GetOrAdd(options.Configuration, (key) =>
        {
            if (options.ConfigurationOptions != null)
                return ConnectionMultiplexer.Connect(options.ConfigurationOptions);
            else
                return ConnectionMultiplexer.Connect(options.Configuration);
        });
    }
}
