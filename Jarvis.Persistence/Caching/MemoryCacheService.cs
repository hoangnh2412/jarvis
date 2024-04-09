using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Jarvis.Persistence.Caching.Interfaces;

namespace Jarvis.Persistence.Caching;

public class MemoryCacheService : MemoryDistributedCache, ICachingService
{
    public MemoryCacheService(
        IOptions<MemoryDistributedCacheOptions> optionsAccessor)
        : base(optionsAccessor)
    {
    }

    public MemoryCacheService(
        IOptions<MemoryDistributedCacheOptions> optionsAccessor,
        ILoggerFactory loggerFactory)
        : base(optionsAccessor, loggerFactory)
    {
    }

    public Task ExecuteCommandAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetKeysAsync(string pattern, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task HashDeleteAsync(string key, string hashField, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        if (items.ContainsKey(hashField))
            items.Remove(hashField);

        await HashSetAsync(key, items, null, token);
    }

    public async Task HashDeleteAsync(string key, string[] hashFields, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);

        foreach (var hashField in hashFields)
        {
            if (items.ContainsKey(hashField))
                items.Remove(hashField);
        }

        await HashSetAsync(key, items, null, token);
    }

    public async Task<Dictionary<string, string>> HashGetAsync(string key, CancellationToken token = default)
    {
        var bytes = await base.GetAsync(key, token);
        if (bytes == null)
            return new Dictionary<string, string>();

        var data = Encoding.UTF8.GetString(bytes);
        if (data == null)
            return new Dictionary<string, string>();

        return JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
    }

    public async Task<string> HashGetAsync(string key, string hashField, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        if (!items.ContainsKey(hashField))
            return null;

        return items[hashField];
    }

    public async Task<List<T>> HashGetAsync<T>(string key, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        return items.Select(x => JsonConvert.DeserializeObject<T>(x.Value)).ToList();
    }

    public async Task<List<T>> HashGetAsync<T>(string key, string[] hashFields, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        return items.Where(x => hashFields.Contains(x.Key)).Select(x => JsonConvert.DeserializeObject<T>(x.Value)).ToList();
    }

    public async Task<List<string>> HashGetAsync(string key, string[] hashFields, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        return items.Where(x => hashFields.Contains(x.Key)).Select(x => x.Value).ToList();
    }

    public async Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken token = default)
    {
        var items = await HashGetAsync(key, token);
        if (!items.ContainsKey(hashField))
            return default(T);

        return JsonConvert.DeserializeObject<T>(items[hashField]);
    }

    public async Task HashSetAsync(string key, Dictionary<string, string> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var json = JsonConvert.SerializeObject(data);
        await base.SetAsync(key, Encoding.UTF8.GetBytes(json), options == null ? new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) } : options, token);
    }

    public async Task HashSetAsync<T>(string key, Dictionary<string, T> data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        await HashSetAsync(key, data, options, token);
    }

    public async Task HashSetAsync(string key, string hashField, string data, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        await HashSetAsync(key, new Dictionary<string, string> {
            { hashField, data }
        }, options, token);
    }

    public Task KeyExistAsync(string pattern, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<string> PopAsync(string key, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> PushAsync(string key, string value, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> PushAsync(string key, string[] values, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var bytes = await GetAsync(key, token);
        if (bytes != null)
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));

        var data = await query.Invoke();
        await SetAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), options, token);
        return data;
    }

    public async Task<T> HashGetAsync<T>(string key, string hashField, Func<Task<T>> query, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var data = await HashGetAsync<T>(key, hashField, token);
        if (data != null)
            return data;

        var item = await query.Invoke();
        await HashSetAsync(key, hashField, JsonConvert.SerializeObject(item), options, token);
        return item;
    }

    public async Task<List<T>> HashGetAsync<T>(string cache, string[] hashFields, Func<Task<List<T>>> query, Func<List<T>, Dictionary<string, T>> parser, DistributedCacheEntryOptions options = null, CancellationToken token = default)
    {
        var data = await HashGetAsync<T>(cache, hashFields, token);
        if (data != null)
            return data;

        var item = await query.Invoke();
        await HashSetAsync(cache, parser.Invoke(item), options, token);
        return item;
    }

    public async Task<T> GetAsync<T>(string cache, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        return await GetAsync<T>(cache, query, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expireTime
        }, token);
    }

    public async Task<T> HashGetAsync<T>(string cache, string hashField, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        return await HashGetAsync(cache, hashField, query, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expireTime
        }, token);
    }
}
