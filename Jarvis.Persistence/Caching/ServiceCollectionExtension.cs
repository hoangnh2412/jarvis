using Microsoft.Extensions.DependencyInjection;
using Jarvis.Persistence.Caching.Interfaces;
using Jarvis.Persistence.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Jarvis.Persistence.Caching.Redis;
using Microsoft.Extensions.Options;

namespace Jarvis.Persistence.Caching;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Register memory cache
    /// </summary>
    /// <param name="services"></param>
    /// <param name="entryOption"></param>
    public static void AddInMemoryCache(this IServiceCollection services, CacheEntryOption entryOption = null)
    {
        services.AddDistributedMemoryCache((options) =>
        {
            options.ExpirationScanFrequency = entryOption == null ? TimeSpan.FromMinutes(15) : TimeSpan.FromSeconds(entryOption.MemoryCacheSeconds);
        });

        services.AddSingleton<ICachingService, MemoryCacheService>();
    }

    /// <summary>
    /// Register distributed cache Redis
    /// </summary>
    /// <param name="services"></param>
    /// <param name="redisOption"></param>
    /// <param name="name"></param>
    public static void AddRedisCache(this IServiceCollection services, RedisOption redisOption, string name = null)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = redisOption.InstanceName;
            options.Configuration = redisOption.Configuration;
        });

        services.AddSingleton<ICachingService, RedisCacheService>((sp) =>
        {
            return new RedisCacheService(Options.Create(redisOption), name);
        });
    }

    public static IServiceCollection AddMultiCache(this IServiceCollection services, IConfiguration congifuration)
    {
        var cachingOption = new CacheOption();
        var cachingSection = congifuration.GetSection("Caching");
        services.Configure<CacheOption>(cachingSection);
        cachingSection.Bind(cachingOption);

        services.AddInMemoryCache();

        if (cachingOption.Redis != null)
        {
            foreach (var item in cachingOption.Redis)
            {
                services.AddRedisCache(item.Value, item.Key);
            }
        }

        services.AddSingleton<IMultiCachingService, MultiCachingService>();
        return services;
    }
}
