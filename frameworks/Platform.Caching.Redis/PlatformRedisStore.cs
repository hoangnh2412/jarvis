using StackExchange.Redis;

namespace Platform.Caching.Redis;

public interface IPlatformRedisStore
{
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    Task SetStringAsync(string key, string value, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<bool> SetStringIfNotExistsAsync(string key, string value, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<bool> CompareAndSetStringAsync(string key, string expectedValue, string newValue, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<bool> DeleteIfValueAsync(string key, string expectedValue, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> FindKeysAsync(string pattern, CancellationToken cancellationToken = default);
}

public sealed class PlatformRedisStore : IPlatformRedisStore
{
    private readonly IConnectionMultiplexer _connection;

    public PlatformRedisStore(string configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration);

        _connection = RedisConnectionManager.GetInstance().Create(
            RedisConnectionPurpose.DistributedCache,
            configuration);
    }

    public async Task<string?> GetStringAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = await _connection.GetDatabase().StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetStringAsync(
        string key,
        string value,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.GetDatabase().StringSetAsync(key, value, expiry);
    }

    public async Task<bool> SetStringIfNotExistsAsync(
        string key,
        string value,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _connection.GetDatabase().StringSetAsync(key, value, expiry, When.NotExists);
    }

    public async Task<bool> CompareAndSetStringAsync(
        string key,
        string expectedValue,
        string newValue,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        const string script = "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('SET', KEYS[1], ARGV[2], 'PX', ARGV[3]) and 1 or 0 else return 0 end";
        var result = await _connection.GetDatabase().ScriptEvaluateAsync(
            script,
            [key],
            [expectedValue, newValue, (long)Math.Max(1, expiry.TotalMilliseconds)]);
        return (int)result == 1;
    }

    public async Task<bool> DeleteIfValueAsync(
        string key,
        string expectedValue,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        const string script = "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('DEL', KEYS[1]) else return 0 end";
        var result = await _connection.GetDatabase().ScriptEvaluateAsync(script, [key], [expectedValue]);
        return (int)result == 1;
    }

    public async Task<bool> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _connection.GetDatabase().KeyDeleteAsync(key);
    }

    public Task<IReadOnlyList<string>> FindKeysAsync(
        string pattern,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var endpoint in _connection.GetEndPoints())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var server = _connection.GetServer(endpoint);
            foreach (var key in server.Keys(pattern: pattern, pageSize: 250))
            {
                cancellationToken.ThrowIfCancellationRequested();
                keys.Add(key.ToString());
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(keys.ToArray());
    }
}
