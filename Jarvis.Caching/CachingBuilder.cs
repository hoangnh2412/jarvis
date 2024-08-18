using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Caching;

public class CachingBuilder
{
    public IServiceCollection Services { get; }
    public CacheOption Config { get; }
    private IMemoryCache? _memoryCache;
    private readonly Dictionary<string, IDistributedCache> _distributedCaches = new Dictionary<string, IDistributedCache>();

    internal CachingBuilder(CacheOption config, IServiceCollection services)
    {
        Services = services;
        Config = config;
    }

    public CachingBuilder SetMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        Services.AddSingleton(_memoryCache);
        return this;
    }

    public CachingBuilder AddDistCache(string disType, IDistributedCache distCache)
    {
        _distributedCaches.Add(disType, distCache);
        return this;
    }

    public ICacheService Build(IServiceProvider serviceProvider)
    {
        var service = new CacheService(Config);
        service.SetServiceProvider(serviceProvider);

        if (_memoryCache is not null)
            service.SetMemCache(_memoryCache);

        service.SetDistributedCaches(_distributedCaches);
        return service;
    }
}