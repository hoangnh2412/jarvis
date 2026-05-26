using Jarvis.Caching.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Caching;

/// <summary>
/// Multi-tier cache: memory first, optional distributed (Redis), with optional loader delegate.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly JarvisCacheOptions _cacheOption;
    private IMemoryCache? _memCache;
    private DistributedCacheRegistry? _distRegistry;
    private IServiceProvider? _serviceProvider;

    public CacheService(JarvisCacheOptions cacheConfig)
    {
        ArgumentNullException.ThrowIfNull(cacheConfig);
        _cacheOption = cacheConfig;
    }

    public async Task<T?> GetAsync<T>(CacheParam param, CancellationToken cancellationToken = default)
    {
        var hit = await TryGetAsync<T>(param, cancellationToken).ConfigureAwait(false);
        return hit.HasValue ? hit.Value : default;
    }

    public Task<CacheValue<T>> TryGetAsync<T>(CacheParam param, CancellationToken cancellationToken = default) =>
        TryGetCoreAsync<T>(param, cancellationToken);

    public async Task<T?> GetOrSetAsync<T>(
        CacheParam param,
        Func<CancellationToken, Task<T>> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var hit = await TryGetCoreAsync<T>(param, cancellationToken).ConfigureAwait(false);
        if (hit.HasValue)
            return hit.Value;

        var data = await query(cancellationToken).ConfigureAwait(false);
        if (data is null && default(T) is null)
            return default;

        await SetAsync(param, data, cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async Task SetAsync<T>(CacheParam param, T data, CancellationToken cancellationToken = default)
    {
        var resolution = Resolve(param);
        if (resolution is null)
            return;

        if (_memCache is not null && resolution.Value.MemoryExpires is not null)
            await _memCache.SetAsync(resolution.Value.Key, data, resolution.Value.MemoryExpires, cancellationToken).ConfigureAwait(false);

        if (!resolution.Value.UseDistributed
            || resolution.Value.DistributedExpires is null
            || !TryGetDistributedCache(resolution.Value.DistributedType, resolution.Value.DistributedGroup, out var distCache)
            || distCache is null)
            return;

        await distCache.SetAsync(resolution.Value.Key, data, resolution.Value.DistributedExpires, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(CacheParam param, CancellationToken cancellationToken = default)
    {
        var resolution = Resolve(param);
        if (resolution is null)
            return;

        if (_memCache is not null)
        {
            await _memCache.RemoveAsync(resolution.Value.Key, cancellationToken).ConfigureAwait(false);
            var publisher = _serviceProvider?.GetService<IMemoryCacheInvalidationPublisher>();
            if (publisher is not null)
            {
                await publisher.PublishAsync(new MemoryCacheInvalidationInfo
                {
                    Key = resolution.Value.Key,
                    MachineName = Environment.MachineName,
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        if (resolution.Value.UseDistributed
            && TryGetDistributedCache(resolution.Value.DistributedType, resolution.Value.DistributedGroup, out var distCache)
            && distCache is not null)
            await distCache.RemoveAsync(resolution.Value.Key, cancellationToken).ConfigureAwait(false);
    }

    internal void SetServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    internal void SetMemCache(IMemoryCache memCache) => _memCache = memCache;

    internal void SetDistributedCacheRegistry(DistributedCacheRegistry registry) =>
        _distRegistry = registry;

    internal CacheItemResolution? Resolve(CacheParam cacheParam)
    {
        if (!_cacheOption.Items.TryGetValue(cacheParam.Name, out var config) || config is null)
            return null;

        var configValues = config.GetConfigValues(cacheParam);
        if (string.IsNullOrEmpty(configValues.Key))
            return null;

        return configValues.ToItemResolution(_cacheOption, cacheParam.Name);
    }

    private async Task<CacheValue<T>> TryGetCoreAsync<T>(CacheParam param, CancellationToken cancellationToken)
    {
        var resolution = Resolve(param);
        if (resolution is null)
            return CacheValue<T>.Miss();

        if (_memCache is not null)
        {
            var cached = await _memCache.TryGetAsync<T>(resolution.Value.Key, cancellationToken).ConfigureAwait(false);
            if (cached.HasValue)
                return cached;
        }

        if (!resolution.Value.UseDistributed
            || !TryGetDistributedCache(resolution.Value.DistributedType, resolution.Value.DistributedGroup, out var distCache)
            || distCache is null)
            return CacheValue<T>.Miss();

        var distCached = await distCache.TryGetAsync<T>(resolution.Value.Key, cancellationToken).ConfigureAwait(false);
        if (!distCached.HasValue)
            return CacheValue<T>.Miss();

        if (_memCache is not null && resolution.Value.MemoryExpires is not null)
            await _memCache.SetAsync(resolution.Value.Key, distCached.Value, resolution.Value.MemoryExpires, cancellationToken).ConfigureAwait(false);

        return distCached;
    }

    private bool TryGetDistributedCache(string distributedType, string distributedGroup, out IDistributedCache? distCache)
    {
        distCache = null;
        if (string.IsNullOrEmpty(distributedType) || string.IsNullOrEmpty(distributedGroup))
            return false;

        if (_distRegistry is null)
            return false;

        return _distRegistry.Caches.TryGetValue($"{distributedType}:{distributedGroup}", out distCache);
    }
}
