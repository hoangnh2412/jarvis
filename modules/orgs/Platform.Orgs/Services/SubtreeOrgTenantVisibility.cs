using Platform.Tenants.Services;

namespace Platform.Orgs.Services;

/// <summary>Org nhìn thấy: tenant đang làm việc và các tenant thấp hơn trong cây catalog.</summary>
public sealed class SubtreeOrgTenantVisibility(ITenantManager tenants) : IOrgTenantVisibility
{
    public async Task<IReadOnlyList<Guid>> ListVisibleTenantIdsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rows = await tenants.ListSubtreeAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return rows.Select(row => row.Id).ToArray();
    }
}
