using Platform.DDD.Domain.Services;
using Platform.Tenants.Services;

namespace Sample.Services;

/// <summary>Profile tenant cho controller org-structure, cùng id header với <see cref="SampleCurrentTenantStore"/>.</summary>
public sealed class SampleTenantCatalogStore : ICurrentTenantStore<TenantCatalogProfile>
{
    public Task<TenantCatalogProfile?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<TenantCatalogProfile?>(new TenantCatalogProfile
        {
            TenantId = tenantId,
            IsActive = true,
        });
    }
}
