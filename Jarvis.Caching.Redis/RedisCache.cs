
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public class RedisCache(
    IConnectionMultiplexer connectionMultiplexer,
    string instanceName) : IDistributedCache
{
    private readonly IConnectionMultiplexer _muxer = connectionMultiplexer;
    private readonly string _instanceName = instanceName;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _muxer.GetDatabase();

        var cached = await db.HashGetAsync(PrefixKey(key), "data");
        if (cached.IsNullOrEmpty || !cached.HasValue)
            return default;

        return JsonConvert.DeserializeObject<T>(cached.ToString());
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _muxer.GetDatabase();

        await db.HashDeleteAsync(PrefixKey(key), "data");
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default)
    {
        if (expires == null)
            return;

        var db = _muxer.GetDatabase();

        await db.HashSetAsync(PrefixKey(key), "data", JsonConvert.SerializeObject(data));
        await db.KeyExpireAsync(PrefixKey(key), expires);
    }

    private string PrefixKey(string key) => $"{_instanceName}:{key}";
}
