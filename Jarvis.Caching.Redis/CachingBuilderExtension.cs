using Jarvis.Caching.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jarvis.Caching.Redis;

public static class CachingBuilderExtension
{
    public static JarvisCachingBuilder UseRedisDistributedCache(this JarvisCachingBuilder builder)
    {
        if (!builder.OptionsSnapshot.DistributedGroups.ContainsKey("Redis"))
            throw new InvalidOperationException("Cache:DistributedGroups:Redis is required for UseRedisDistributedCache.");

        builder.HostBuilder.Services.AddSingleton<IConfigureOptions<DistributedCacheRegistry>>(
            _ => new ConfigureRedisDistributedCaches(builder.OptionsSnapshot));

        return builder;
    }

    /// <summary>
    /// Registers a dedicated Redis pub/sub connection for cross-node memory invalidation
    /// (<see cref="MemoryCacheInvalidationDefaults.ConnectionServiceKey"/>), separate from distributed cache clusters.
    /// </summary>
    /// <param name="configuration">
    /// StackExchange.Redis configuration string. When null, uses
    /// <c>Cache:MemoryInvalidation:Redis:Configuration</c>.
    /// </param>
    public static JarvisCachingBuilder UseRedisMemoryCacheInvalidation(
        this JarvisCachingBuilder builder,
        string? configuration = null)
    {
        var resolved = RedisMemoryCacheInvalidationRegistration.ResolveConfiguration(
            builder.HostBuilder.Configuration,
            builder.OptionsSnapshot,
            configuration);

        RedisMemoryCacheInvalidationRegistration.Register(builder.HostBuilder.Services, resolved);
        return builder;
    }
}
