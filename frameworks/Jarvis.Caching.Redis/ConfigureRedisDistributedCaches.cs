using Microsoft.Extensions.Options;

namespace Jarvis.Caching.Redis;

internal sealed class ConfigureRedisDistributedCaches(
    JarvisCacheOptions options)
    : IConfigureOptions<DistributedCacheRegistry>
{
    private readonly JarvisCacheOptions _options = options;

    public void Configure(DistributedCacheRegistry registry)
    {
        if (!_options.DistributedGroups.TryGetValue("Redis", out var redisSection))
            return;

        foreach (var group in redisSection)
        {
            var cacheKey = $"Redis:{group.Key}";
            if (registry.Caches.ContainsKey(cacheKey))
                continue;

            registry.Caches[cacheKey] = new RedisCache(
                RedisConnectionManager.GetInstance().Create(
                    RedisConnectionPurpose.DistributedCache,
                    group.Value["Configuration"]),
                group.Value["InstanceName"]);
        }
    }
}
