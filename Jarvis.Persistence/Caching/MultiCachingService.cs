using System.Text;
using Jarvis.Persistence.Caching.Interfaces;
using Jarvis.Persistence.Caching.Memory;
using Jarvis.Persistence.Caching.Redis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Jarvis.Persistence.Caching;

public class MultiCachingService : IMultiCachingService
{
    private readonly CacheOption _cacheOption;
    private readonly MemoryCacheService _memoryCacheService;
    private readonly IEnumerable<ICachingService> _distributedCacheService;

    public MultiCachingService(
        IEnumerable<ICachingService> distributedCacheServices,
        IOptions<CacheOption> cacheOption)
    {
        _cacheOption = cacheOption.Value;
        _memoryCacheService = (MemoryCacheService)distributedCacheServices.FirstOrDefault(x => x.GetType().AssemblyQualifiedName == typeof(MemoryCacheService).AssemblyQualifiedName);
        _distributedCacheService = distributedCacheServices.Where(x => x.GetType().AssemblyQualifiedName != typeof(MemoryCacheService).AssemblyQualifiedName && x.GetType().Name.StartsWith(_cacheOption.DistributedType));
    }

    public async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> query, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        var entry = _cacheOption.Entries.FirstOrDefault(x => x.Key == cacheKey);
        if (entry == null)
            return default(T);

        var policy = ParsePolicy(entry, expireTime);

        if (policy.MemoryCacheSeconds > 0)
        {
            var bytes = await _memoryCacheService.GetAsync(cacheKey, token);
            if (bytes != null)
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }

        T item = default(T);
        if (policy.DistributedCacheSeconds > 0)
        {
            var distributedCacheService = (RedisCacheService)_distributedCacheService.FirstOrDefault(x => x.Name == entry.StorageLocation);
            if (distributedCacheService == null)
                distributedCacheService = (RedisCacheService)_distributedCacheService.FirstOrDefault(x => x.Name == "Default");

            item = await distributedCacheService.GetAsync<T>(cacheKey, query, TimeSpan.FromSeconds(policy.DistributedCacheSeconds), token);
        }

        if (item == null)
            return item;

        await _memoryCacheService.SetAsync<T>(cacheKey, item, TimeSpan.FromSeconds(policy.MemoryCacheSeconds), token);

        return item;
    }

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expireTime = null, CancellationToken token = default)
    {
        var entry = _cacheOption.Entries.FirstOrDefault(x => x.Key == cacheKey);
        if (entry == null)
            return;

        var policy = ParsePolicy(entry, expireTime);

        if (entry.MemoryCacheSeconds > 0)
            await _memoryCacheService.SetAsync<T>(cacheKey, value, TimeSpan.FromSeconds(entry.MemoryCacheSeconds), token);

        if (entry.DistributedCacheSeconds > 0)
        {
            var distributedCacheService = (RedisCacheService)_distributedCacheService.FirstOrDefault(x => x.Name == entry.StorageLocation);
            await distributedCacheService.SetAsync<T>(cacheKey, value, TimeSpan.FromSeconds(policy.DistributedCacheSeconds), token);
        }
    }

    private PolicyEntryOption ParsePolicy(CacheEntryOption entry, TimeSpan? expireTime)
    {
        // If not configure Default policy section in appsettings => DistributedCacheSeconds = 3600, MemoryCacheSeconds = 60
        PolicyEntryOption policy = new PolicyEntryOption();

        // Override default: If entry configure ExpirePolicy => Use config Policy
        if (!string.IsNullOrEmpty(entry.ExpirePolicy) && _cacheOption.Policies.ContainsKey(entry.ExpirePolicy))
            policy = _cacheOption.Policies[entry.ExpirePolicy];
        else if (_cacheOption.Policies.ContainsKey("Default"))
            policy = _cacheOption.Policies["Default"];

        // Override config: If entry configure MemoryCacheSeconds, DistributedCacheSeconds => Use inline configure
        if (entry.MemoryCacheSeconds > 0)
            policy.MemoryCacheSeconds = entry.MemoryCacheSeconds;

        if (entry.DistributedCacheSeconds > 0)
            policy.DistributedCacheSeconds = entry.DistributedCacheSeconds;

        // Override config by params
        if (expireTime.HasValue)
        {
            policy.MemoryCacheSeconds = (int)expireTime.Value.TotalSeconds;
            policy.DistributedCacheSeconds = (int)expireTime.Value.TotalSeconds;
        }

        return policy;
    }
}
