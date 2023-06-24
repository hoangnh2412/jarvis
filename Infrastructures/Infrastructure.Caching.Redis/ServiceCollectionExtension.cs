using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.Caching.Redis
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add Redis to DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="redisOption"></param>
        /// <param name="cacheOptions">Default cache in 15 minutes</param>
        public static void AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> redisOption, Action<DistributedCacheEntryOptions> cacheOptions = null)
        {
            if (cacheOptions == null)
            {
                services.Configure<DistributedCacheEntryOptions>(options =>
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                });
            }
            else
            {
                services.Configure<DistributedCacheEntryOptions>(cacheOptions);
            }

            services.AddStackExchangeRedisCache(redisOption);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        public static void AddRedisCache(this IServiceCollection services, RedisOption redisOption)
        {
            services.AddRedisCache(options =>
            {
                options.InstanceName = redisOption.InstanceName;
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
                {
                    Password = redisOption.Password,
                    ConnectRetry = redisOption.ConnectRetry,
                    AbortOnConnectFail = redisOption.AbortOnConnectFail,
                    ConnectTimeout = redisOption.ConnectTimeout,
                    SyncTimeout = redisOption.SyncTimeout,
                    DefaultDatabase = redisOption.DefaultDatabase,
                };

                if (redisOption.EndPoints != null)
                {
                    foreach (var item in redisOption.EndPoints)
                    {
                        options.ConfigurationOptions.EndPoints.Add(item);
                    }
                }
            });
        }
    }
}
