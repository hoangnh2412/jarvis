using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
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
        public static void AddRedisCache(this IServiceCollection services, RedisOption redisOption, Action<DistributedCacheEntryOptions> cacheOptions = null)
        {
            var config = new ConfigurationOptions
            {
                Password = redisOption.Password,
                ConnectRetry = redisOption.ConnectRetry,
                AbortOnConnectFail = redisOption.AbortOnConnectFail,
                ConnectTimeout = redisOption.ConnectTimeout,
                SyncTimeout = redisOption.SyncTimeout,
                DefaultDatabase = redisOption.DefaultDatabase
            };

            foreach (var item in redisOption.EndPoints)
            {
                config.EndPoints.Add(item);
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = redisOption.InstanceName;
                options.ConfigurationOptions = config;
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.Configure<DistributedCacheEntryOptions>(options =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            });
        }
    }
}
