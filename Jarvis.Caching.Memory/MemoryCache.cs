
using Microsoft.Extensions.Caching.Memory;

namespace Jarvis.Caching.Memory;

public class MemoryCache(
    Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    : IMemoryCache
{
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache = cache;

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.Get<T>(key));
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default)
    {
        if (expires == null)
            return Task.CompletedTask;

        _cache.Set(key, data, new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = expires.Value
        });
        return Task.CompletedTask;
    }
}
