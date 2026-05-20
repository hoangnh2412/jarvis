using System.Collections.Concurrent;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public interface IRedisConnectionManager
{
    IConnectionMultiplexer Create(RedisConnectionPurpose purpose, string configuration);

    Task<IConnectionMultiplexer> CreateAsync(RedisConnectionPurpose purpose, string configuration);

    IConnectionMultiplexer GetConnection(RedisConnectionPurpose purpose, string configuration);
}

public sealed class RedisConnectionManager : IRedisConnectionManager
{
    private readonly ConcurrentDictionary<string, Lazy<IConnectionMultiplexer>> _connections = new(StringComparer.Ordinal);

    public static IRedisConnectionManager Instance { get; } = new RedisConnectionManager();

    public static IRedisConnectionManager GetInstance() => Instance;

    public IConnectionMultiplexer Create(RedisConnectionPurpose purpose, string configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return _connections.GetOrAdd(BuildKey(purpose, configuration), static key =>
            new Lazy<IConnectionMultiplexer>(() =>
            {
                var config = key[(key.IndexOf('\0', StringComparison.Ordinal) + 1)..];
                return ConnectionMultiplexer.Connect(config);
            })).Value;
    }

    public Task<IConnectionMultiplexer> CreateAsync(RedisConnectionPurpose purpose, string configuration)
    {
        return Task.FromResult(Create(purpose, configuration));
    }

    public IConnectionMultiplexer GetConnection(RedisConnectionPurpose purpose, string configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (_connections.TryGetValue(BuildKey(purpose, configuration), out var lazy))
            return lazy.Value;

        throw new InvalidOperationException(
            $"No Redis connection registered for purpose '{purpose}' and configuration '{configuration}'. Call Create or CreateAsync first.");
    }

    internal static string BuildKey(RedisConnectionPurpose purpose, string configuration) =>
        $"{purpose}\0{configuration}";
}
