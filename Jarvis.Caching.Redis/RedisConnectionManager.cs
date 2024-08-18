using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public interface IRedisConnectionManager
{
    IConnectionMultiplexer Create(string configuration);

    Task<IConnectionMultiplexer> CreateAsync(string configuration);

    IConnectionMultiplexer GetConnection(string configuration);

}

public class RedisConnectionManager : IRedisConnectionManager
{
    private readonly Dictionary<string, IConnectionMultiplexer> Connections = new();
    private readonly static object syncLock = new();
    private readonly static SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly static object singletonLock = new();
    private static IRedisConnectionManager? singleton;

    public static IRedisConnectionManager GetInstance()
    {
        if (singleton == null)
        {
            lock (singletonLock)
            {
                if (singleton == null)
                {
                    singleton = new RedisConnectionManager();
                    return singleton;
                }
            }
        }
        return singleton;
    }

    public IConnectionMultiplexer Create(string configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        if (!Connections.ContainsKey(configuration))
        {
            lock (syncLock)
            {
                if (!Connections.ContainsKey(configuration))
                {
                    var muxer = ConnectionMultiplexer.Connect(configuration);
                    Connections.Add(configuration, muxer);
                    return muxer;
                }
            }
        }
        return Connections[configuration];
    }

    public async Task<IConnectionMultiplexer> CreateAsync(string configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        if (!Connections.ContainsKey(configuration))
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!Connections.ContainsKey(configuration))
                {
                    var muxer = ConnectionMultiplexer.Connect(configuration);
                    Connections.Add(configuration, muxer);
                    return muxer;
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        return Connections[configuration];
    }

    public IConnectionMultiplexer GetConnection(string configuration)
    {
        Connections.TryGetValue(configuration, out var connectionMultiplexer);

        if (connectionMultiplexer is null)
            throw new NullReferenceException(nameof(connectionMultiplexer));

        return connectionMultiplexer;
    }

}