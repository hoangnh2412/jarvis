using Jarvis.Caching;
using Jarvis.Domain.DataStorages;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Default cache layer for <see cref="ITenantConnectionStringResolver"/>.
/// Memory vs distributed tiers and TTL come from <c>Cache:Items</c> (e.g. <c>ConnectionString</c> with <c>MemSeconds</c> / <c>DistributedSeconds</c>).
/// </summary>
/// <remarks>
/// <paramref name="inner"/> is the host-defined fallback on cache miss — configuration, EF master lookup, HTTP API, MinIO metadata, etc.
/// Register any <see cref="ITenantConnectionStringResolver"/> via <c>AddCoreDbContext&lt;TDb, TInner&gt;</c>; this decorator is applied automatically.
/// </remarks>
public sealed class CachingTenantConnectionStringResolver(
    ITenantConnectionStringResolver inner,
    ICacheService cacheService,
    string cacheItemName = "ConnectionString",
    string parameterName = "dbid")
    : ITenantConnectionStringResolver
{
    /// <summary>Default <c>Cache:Items</c> name (Sample: <c>ConnectionString</c>).</summary>
    public const string DefaultCacheItemName = "ConnectionString";

    /// <summary>Placeholder in the item key template (Sample: <c>conn:{dbid}</c>).</summary>
    public const string DefaultParameterName = "dbid";

    private readonly ITenantConnectionStringResolver _inner = inner;
    private readonly ICacheService _cacheService = cacheService;
    private readonly string _cacheItemName = cacheItemName;
    private readonly string _parameterName = parameterName;

    public Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return _inner.GetConnectionStringAsync(name, cancellationToken);

        var param = CacheParam.Create(_cacheItemName).WithParam(_parameterName, name);

        return _cacheService.GetOrSetAsync(
            param,
            ct => _inner.GetConnectionStringAsync(name, ct),
            cancellationToken);
    }
}
