using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.Caching.Redis;

public static class CachingBuilderExtension
{
    public static CachingBuilder UseDistributedRedisCache(this CachingBuilder builder)
    {
        if (!builder.Config.DistGroups.ContainsKey("Redis"))
            throw new InvalidOperationException("Can't use distributed redis cache, no configuration found!");

        var redisSection = builder.Config.DistGroups["Redis"];
        foreach (var group in redisSection)
        {
            builder.AddDistCache(
                $"Redis:{group.Key}",
                new RedisCache(
                    RedisConnectionManager.GetInstance().Create(group.Value["Configuration"]),
                    group.Value["InstanceName"]
                )
            );
        }
        return builder;
    }

    public static CachingBuilder UseRedisMemCacheInvalidation(this CachingBuilder builder, string configuration)
    {
        builder.Services.AddSingleton<IMemoryCacheInvalidatorPublisher>(sp =>
        {
            return new RedisMemoryCacheInvalidatorPublisher(configuration);
        });

        builder.Services.AddHostedService(sp =>
        {
            return new RedisMemoryCacheInvalidator(
                configuration,
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ILogger<RedisMemoryCacheInvalidator>>()
            );
        });
        return builder;
    }
}