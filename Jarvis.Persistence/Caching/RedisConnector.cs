using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace Jarvis.Persistence.Caching;

public static class RedisConnector
{
    private static IConnectionMultiplexer _connection;
    private static IDatabase _database;
    private static SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    public static async Task<IConnectionMultiplexer> ConnectAsync(RedisCacheOptions options, CancellationToken token = default(CancellationToken))
    {
        await _connectionLock.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (_connection != null)
                return _connection;

            if (options.ConfigurationOptions != null)
                _connection = await ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions).ConfigureAwait(false);
            else
                _connection = await ConnectionMultiplexer.ConnectAsync(options.Configuration).ConfigureAwait(false);
        }
        finally
        {
            _connectionLock.Release();
        }
        return _connection;
    }

    public static IDatabase GetDatabase()
    {
        if (_database != null)
            return _database;

        _database = _connection.GetDatabase();
        return _database;
    }
}
