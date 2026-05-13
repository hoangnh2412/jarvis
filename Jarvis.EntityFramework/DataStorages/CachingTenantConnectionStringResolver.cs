using Jarvis.Domain.DataStorages;
using Microsoft.Extensions.Caching.Memory;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Caches connection strings by lookup name to reduce repeated configuration hits for dedicated-database tenants.
/// </summary>
public sealed class CachingTenantConnectionStringResolver(
    ITenantConnectionStringResolver inner,
    IMemoryCache cache,
    TimeSpan? slidingExpiration = null)
    : ITenantConnectionStringResolver
{
    private readonly ITenantConnectionStringResolver _inner = inner;
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _slidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(10);

    public string GetConnectionString(string? name = null)
    {
        if (string.IsNullOrEmpty(name))
            return _inner.GetConnectionString(name);

        var key = (object)("Jarvis:ConnStr:" + name);
        return _cache.GetOrCreate(key, e =>
        {
            e.SlidingExpiration = _slidingExpiration;
            return _inner.GetConnectionString(name);
        })!;
    }

    public async Task<string> GetConnectionStringAsync(string? name = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return await _inner.GetConnectionStringAsync(name, cancellationToken).ConfigureAwait(false);

        var key = (object)("Jarvis:ConnStr:" + name);
        return (await _cache.GetOrCreateAsync(
            key,
            async e =>
            {
                e.SlidingExpiration = _slidingExpiration;
                return await _inner.GetConnectionStringAsync(name, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false))!;
    }
}
