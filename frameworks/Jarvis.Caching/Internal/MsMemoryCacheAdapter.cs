using Microsoft.Extensions.Caching.Memory;

namespace Jarvis.Caching.Internal;

internal sealed class MsMemoryCacheAdapter(Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : IMemoryCache
{
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache = cache;

    public Task<CacheValue<T>> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? value))
            return Task.FromResult(CacheValue<T>.Hit(value));

        return Task.FromResult(CacheValue<T>.Miss());
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default)
    {
        if (expires is null)
            return Task.CompletedTask;

        _cache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expires.Value
        });
        return Task.CompletedTask;
    }
}
