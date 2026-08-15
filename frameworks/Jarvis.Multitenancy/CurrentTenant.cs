using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Services;

namespace Jarvis.Multitenancy;

/// <summary>
/// API tenant công khai — <see cref="GetIdAsync"/> (R2) và <see cref="GetAsync"/> (store).
/// </summary>
public sealed class CurrentTenant<TTenant>(
    ICurrentTenantAccessor currentTenantAccessor,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantStore<TTenant> tenantStore) : ICurrentTenant<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    private Guid? _resolvedTenantId;
    private bool _resolved;

    private Guid? _cachedProfileId;
    private TTenant? _profile;
    private bool _profileLoaded;

    public async Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default)
    {
        var ambient = currentTenantAccessor.TenantId;
        if (ambient.HasValue)
            return ambient;

        if (_resolved)
            return _resolvedTenantId;

        _resolvedTenantId = await tenantIdResolverFactory
            .GetTenantIdAsync(cancellationToken)
            .ConfigureAwait(false);
        _resolved = true;
        return _resolvedTenantId;
    }

    public async Task<TTenant?> GetAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = await GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (!tenantId.HasValue)
        {
            _profileLoaded = true;
            _cachedProfileId = null;
            _profile = null;
            return null;
        }

        if (_profileLoaded && _cachedProfileId == tenantId)
            return _profile;

        _profile = await tenantStore
            .FindAsync(tenantId.Value, cancellationToken)
            .ConfigureAwait(false);
        _cachedProfileId = tenantId;
        _profileLoaded = true;
        return _profile;
    }

    public IDisposable Change(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        return currentTenantAccessor.BeginScope(tenantId);
    }
}
