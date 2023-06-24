using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.Caching.InMemory
{
    public static class ServiceCollectionExtension
    {
        public static void AddInMemoryCache(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache((options) =>
            {
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(15);
            });
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
    }
}
