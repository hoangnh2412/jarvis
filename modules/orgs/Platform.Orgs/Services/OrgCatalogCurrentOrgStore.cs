using Platform.DDD.Domain.Services;
using Platform.Orgs.Abstractions;

namespace Platform.Orgs.Services;

/// <summary><see cref="ICurrentOrgStore{TOrg}"/> đọc catalog qua <see cref="IOrgCatalog"/> + ambient tenant.</summary>
public sealed class OrgCatalogCurrentOrgStore<TTenant> : ICurrentOrgStore<OrgCatalogOrgProfile>
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly IOrgCatalog _catalog;
    private readonly ICurrentTenant<TTenant> _currentTenant;

    public OrgCatalogCurrentOrgStore(IOrgCatalog catalog, ICurrentTenant<TTenant> currentTenant)
    {
        _catalog = catalog;
        _currentTenant = currentTenant;
    }

    public async Task<OrgCatalogOrgProfile?> FindAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        var tenantId = await _currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            return null;

        var descriptor = await _catalog.GetAsync(orgId, tenantId.Value, cancellationToken).ConfigureAwait(false);
        if (descriptor is null)
            return null;

        return new OrgCatalogOrgProfile
        {
            OrgId = descriptor.Id,
            Code = descriptor.Code,
            Name = descriptor.Name,
        };
    }
}
