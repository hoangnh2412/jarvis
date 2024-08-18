using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Caching;

public class CacheService : ICacheService
{
    private readonly CacheOption _cacheOption;
    private IMemoryCache? _memCache;
    private readonly Dictionary<string, IDistributedCache> _distCaches = new Dictionary<string, IDistributedCache>();
    private IServiceProvider? _serviceProvider;

    internal CacheService(CacheOption cacheConfig)
    {
        _cacheOption = cacheConfig;
    }

    public async virtual Task<T?> GetAsync<T>(CacheParam param, CancellationToken cancellationToken = default)
    {
        var values = GetCacheItemValues(param);
        if (values == null)
            return default;

        var (key, distType, distGroup, memEx, _) = values.Value;
        if (_memCache is not null)
        {
            var cached = await _memCache.GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;
        }

        if (!HasDistCache(distType, distGroup))
            return default;

        var distCache = GetDistCache(distType, distGroup);
        var distCached = await distCache.GetAsync<T>(key, cancellationToken);
        if (distCached == null)
            return default;

        if (_memCache is not null && memEx is not null)
            await _memCache.SetAsync(key, distCached, memEx, cancellationToken);

        return distCached;
    }

    public async virtual Task<T?> GetAsync<T>(CacheParam param, Func<Task<T>> query, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(param, cancellationToken);
        if (cached != null)
            return cached;

        var data = await query.Invoke();
        if (data == null)
            return default;

        await SetAsync(param, data, cancellationToken);
        return data;
    }

    public async virtual Task SetAsync<T>(CacheParam param, T data, CancellationToken cancellationToken = default)
    {
        var values = GetCacheItemValues(param);
        if (values == null)
            return;

        var (key, distType, distGroup, memEx, distEx) = values.Value;

        if (_memCache is not null && memEx is not null)
            await _memCache.SetAsync(key, data, memEx, cancellationToken);

        if (!CanSetDistCache(distType, distGroup, distEx))
            return;

        var distCache = GetDistCache(distType, distGroup);
        await distCache.SetAsync(key, data, distEx, cancellationToken);
    }

    public async virtual Task RemoveAsync(CacheParam param, CancellationToken cancellationToken = default)
    {
        var values = GetCacheItemValues(param);
        if (values == null) return;
        var (key, distType, distGroup, _, _) = values.Value;

        if (_memCache is not null)
        {
            await _memCache.RemoveAsync(key, cancellationToken);
            var publishser = _serviceProvider?.GetService<IMemoryCacheInvalidatorPublisher>();
            if (publishser is not null)
            {
                await publishser.PublishAsync(new MemoryCacheInvalidationInfo()
                {
                    Key = key,
                    MachineName = Environment.MachineName,
                });
            }
        }

        if (HasDistCache(distType, distGroup))
        {
            var distCache = GetDistCache(distType, distGroup);
            await distCache.RemoveAsync(key, cancellationToken);
        }
    }

    private bool HasDistCache(string distType, string distGroup) => _distCaches.ContainsKey($"{distType}:{distGroup}");

    private IDistributedCache GetDistCache(string distType, string distGroup) => _distCaches[$"{distType}:{distGroup}"];

    private bool CanSetDistCache(string distType, string distGroup, TimeSpan? distEx) => HasDistCache(distType, distGroup) && distEx is not null;

    internal void SetServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    internal void SetMemCache(IMemoryCache memCache) => _memCache = memCache;

    internal void AddDistCache(string distType, IDistributedCache distCache) => _distCaches[distType] = distCache;

    internal void SetDistributedCaches(Dictionary<string, IDistributedCache> distCaches)
    {
        foreach (var key in distCaches.Keys)
        {
            _distCaches[key] = distCaches[key];
        }
    }

    protected (string, string, string, TimeSpan?, TimeSpan?)? GetCacheItemValues(CacheParam cacheParam)
    {
        if (!_cacheOption.Items.TryGetValue(cacheParam.Name, out var config))
            return null;

        if (config is null)
            return null;

        var (key, distType, distGroup, memEx, distEx) = config.GetConfigValues(cacheParam);
        if (string.IsNullOrEmpty(key))
            return null;

        if (string.IsNullOrEmpty(distType) || string.IsNullOrEmpty(distGroup))
            return null;

        if (string.IsNullOrEmpty(distType))
            distType = _cacheOption.DefaultDistributedType;

        if (string.IsNullOrEmpty(distGroup))
            distGroup = _cacheOption.DefaultDistributedGroup;

        return (key, distType, distGroup, memEx, distEx);
    }

}