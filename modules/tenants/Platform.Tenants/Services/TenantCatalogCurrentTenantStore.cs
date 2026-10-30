using Platform.DDD.Domain.Services;

namespace Platform.Tenants.Services;

/// <summary>Đọc profile tenant đang làm việc từ catalog Master.</summary>
public sealed class TenantCatalogCurrentTenantStore(ITenantManager tenants) : ICurrentTenantStore<TenantCatalogProfile>
{
    public async Task<TenantCatalogProfile?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var descriptor = await tenants.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (descriptor is null)
            return null;

        return new TenantCatalogProfile
        {
            TenantId = descriptor.Id,
            Code = descriptor.Code,
            Name = descriptor.Name,
            ParentId = descriptor.ParentId,
            IsActive = descriptor.IsActive,
        };
    }
}
