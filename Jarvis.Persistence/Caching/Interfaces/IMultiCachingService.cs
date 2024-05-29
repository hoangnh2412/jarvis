namespace Jarvis.Persistence.Caching.Interfaces;

/// <summary>
/// The interface provider functions get and set data use multi layer caching.
/// Layer 1: Memory cache
/// Layer 2: Distributed cache
/// Layer 3: Database
/// Cache managed by each entry use CacheEntryOption model
/// If cache entry configure MemoryCacheSeconds > 0, priority will be get data from Memory cache.
/// Then if configure DistributedCacheSeconds > 0, will get data from Distributed cache.
/// And finally will get data from Database.
/// </summary>
public interface IMultiCachingService
{
    Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default);

    Task SetAsync<T>(string cacheKey, T value, TimeSpan? expireTime = null, CancellationToken token = default);
}