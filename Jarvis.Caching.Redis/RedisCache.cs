using System.Text.Json;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public sealed class RedisCache(IConnectionMultiplexer connectionMultiplexer, string instanceName) : IDistributedCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IConnectionMultiplexer _muxer = connectionMultiplexer;
    private readonly string _instanceName = instanceName;

    public async Task<CacheValue<T>> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var db = _muxer.GetDatabase();
        var cached = await db.HashGetAsync(PrefixKey(key), "data").ConfigureAwait(false);
        if (cached.IsNullOrEmpty || !cached.HasValue)
            return CacheValue<T>.Miss();

        var value = JsonSerializer.Deserialize<T>(cached.ToString(), SerializerOptions);
        if (value is null && IsNonNullableValueType(typeof(T)))
            return CacheValue<T>.Miss();

        return CacheValue<T>.Hit(value);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var db = _muxer.GetDatabase();
        await db.HashDeleteAsync(PrefixKey(key), "data").ConfigureAwait(false);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default)
    {
        if (expires is null)
            return;

        cancellationToken.ThrowIfCancellationRequested();
        var db = _muxer.GetDatabase();
        var payload = JsonSerializer.Serialize(data, SerializerOptions);
        await db.HashSetAsync(PrefixKey(key), "data", payload).ConfigureAwait(false);
        await db.KeyExpireAsync(PrefixKey(key), expires).ConfigureAwait(false);
    }

    private string PrefixKey(string key) => $"{_instanceName}:{key}";

    private static bool IsNonNullableValueType(Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) is null;
}
