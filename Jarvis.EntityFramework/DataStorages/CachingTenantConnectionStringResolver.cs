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

    public async Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        var key = "Jarvis:ConnStr:" + name;
        return await _cache.GetOrCreateAsync(
            key,
            async e =>
            {
                e.SlidingExpiration = _slidingExpiration;
                return await _inner.GetConnectionStringAsync(name, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}
