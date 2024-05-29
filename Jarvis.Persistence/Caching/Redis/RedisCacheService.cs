using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using Jarvis.Persistence.Caching.Interfaces;

namespace Jarvis.Persistence.Caching.Redis;

public class RedisCacheService : ICachingService
{
    private readonly RedisOption _options;
    public string Name { get; private set; }

    public RedisCacheService(
        IOptions<RedisOption> optionsAccessor,
        string name = "Default")
    {
        Name = name;

        if (optionsAccessor == null)
            throw new ArgumentNullException(nameof(optionsAccessor));

        _options = optionsAccessor.Value;
    }

    public async Task<Dictionary<string, string>> HashGetAsync(string key, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAllAsync(key);
        return data.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
    }

    public async Task<string> HashGetAsync(string key, string hashField, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAsync(key, hashField);
        if (string.IsNullOrEmpty(data.ToString()))
            return default;

        return data.ToString();
    }

    public async Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAllAsync(key);
        return data.Select(x => JsonConvert.DeserializeObject<T>(x.Value.ToString())).ToList();
    }

    public async Task<List<T>> HashGetAsync<T>(string key, string[] hashFields, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAsync(key, hashFields.Select(x => RedisValue.Unbox(x)).ToArray());
        var items = new List<T>();
        foreach (var item in data)
        {
            var element = item.ToString();
            if (!string.IsNullOrEmpty(element))
                items.Add(JsonConvert.DeserializeObject<T>(element));
        }
        return items;
    }

    public async Task<List<string>> HashGetAsync(string key, string[] hashFields, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAsync(key, hashFields.Select(x => RedisValue.Unbox(x)).ToArray());
        var items = new List<string>();
        foreach (var item in data)
        {
            var element = item.ToString();
            if (!string.IsNullOrEmpty(element))
                items.Add(element);
        }
        return items;
    }

    public async Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        var data = await database.HashGetAsync(key, hashField);
        if (string.IsNullOrEmpty(data.ToString()))
            return default;

        return JsonConvert.DeserializeObject<T>(data.ToString());
    }

    public async Task HashSetAsync<T>(string key, Dictionary<string, T> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.HashSetAsync(key, data.Select(x => new HashEntry(x.Key, JsonConvert.SerializeObject(x.Value))).ToArray());
        await database.KeyExpireAsync(key, ParseExpiry(options));
    }

    public async Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.HashSetAsync(key, data.Select(x => new HashEntry(x.Key, x.Value)).ToArray());
        await database.KeyExpireAsync(key, ParseExpiry(options));
    }

    public async Task HashSetAsync(string key, string hashField, string data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.HashSetAsync(key, hashField, data);
        await database.KeyExpireAsync(key, ParseExpiry(options));
    }

    public async Task HashDeleteAsync(string key, string hashField, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.HashDeleteAsync(key, hashField);
    }

    public async Task HashDeleteAsync(string key, string[] hashFields, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.HashDeleteAsync(key, hashFields.Select(x => RedisValue.Unbox(x)).ToArray());
    }

    public async Task<long> PushAsync(string key, string value, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        return await database.ListRightPushAsync(key, value);
    }

    public async Task<long> PushAsync(string key, string[] values, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        return await database.ListRightPushAsync(key, values.Select(x => RedisValue.Unbox(x)).ToArray());
    }

    public async Task<string> PopAsync(string key, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        return await database.ListLeftPopAsync(key);
    }

    public async Task<T> HashGetAsync<T>(string cache, string hashField, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        return await HashGetAsync(cache, hashField, query, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expireTime
        }, token);
    }

    public async Task<T> HashGetAsync<T>(string cache, string hashField, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var data = await HashGetAsync<T>(cache, hashField, token);
        if (data != null)
            return data;

        var item = await query.Invoke();
        await HashSetAsync(cache, hashField, JsonConvert.SerializeObject(item), options, token);
        return item;
    }

    public async Task<List<T>> HashGetAsync<T>(string cache, string[] hashFields, Func<Task<List<T>>> query, Func<List<T>, Dictionary<string, T>> parser, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var data = await HashGetAsync<T>(cache, hashFields, token);
        if (data != null && data.Count == hashFields.Length)
            return data;

        var item = await query.Invoke();
        await HashSetAsync(cache, parser.Invoke(item), options, token);
        return item;
    }

    public byte[] Get(string key)
    {
        return this.GetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        string value = await database.StringGetAsync(key);
        if (string.IsNullOrEmpty(value))
            return null;

        return Encoding.UTF8.GetBytes(value);
    }

    public async Task<T> GetAsync<T>(string cache, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        return await GetAsync<T>(cache, query, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expireTime
        }, token);
    }

    public async Task<T> GetAsync<T>(string cache, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var data = await GetAsync<T>(cache, token);
        if (data != null)
            return data;

        data = await query.Invoke();
        await SetAsync(cache, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), options, token);
        return data;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();
        await database.StringSetAsync(key, Encoding.UTF8.GetString(value), ParseExpiry(options));
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        RemoveAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        key = EnrichKey(key);
        var database = GetDatabase();

        await database.KeyDeleteAsync(key);
    }

    private static TimeSpan? ParseExpiry(DistributedCacheEntryOptions options)
    {
        TimeSpan? expiry = null;
        if (options != null)
        {
            if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value < DateTimeOffset.UtcNow)
                    throw new Exception("AbsoluteExpiration need to be greater then the DateTime.UtcNow");

                expiry = options.AbsoluteExpiration.Value - DateTimeOffset.UtcNow;
            }

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                expiry = options.AbsoluteExpirationRelativeToNow.Value;

            if (options.SlidingExpiration.HasValue)
                expiry = options.SlidingExpiration.Value;
        }

        return expiry;
    }

    private string EnrichKey(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (!key.StartsWith(_options.InstanceName))
            key = $"{_options.InstanceName}:{key}";
        return key;
    }

    private IDatabase GetDatabase(CancellationToken token = default(CancellationToken))
    {
        token.ThrowIfCancellationRequested();

        var connection = RedisConnectionFactory.Connect(new RedisCacheOptions
        {
            InstanceName = _options.InstanceName,
            Configuration = _options.Configuration
        }, token);

        return connection.GetDatabase();
    }

    public Task ExecuteCommandAsync(string key)
    {
        throw new NotImplementedException();
    }

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        await SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expireTime
        }, token);
    }

    public async Task<T> GetAsync<T>(string cache, CancellationToken token = default)
    {
        var bytes = await GetAsync(cache, token);
        if (bytes != null)
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));

        return default(T);
    }
}
