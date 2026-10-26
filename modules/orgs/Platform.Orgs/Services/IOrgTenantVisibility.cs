namespace Platform.Orgs.Services;

/// <summary>Tenant hiện tại và các tenant con được thấy catalog Org.</summary>
public interface IOrgTenantVisibility
{
    Task<IReadOnlyList<Guid>> ListVisibleTenantIdsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed class SelfOrgTenantVisibility : IOrgTenantVisibility
{
    public Task<IReadOnlyList<Guid>> ListVisibleTenantIdsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([tenantId]);
}
