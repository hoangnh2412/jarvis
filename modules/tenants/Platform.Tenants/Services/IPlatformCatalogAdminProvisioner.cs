using Platform.Tenants.Models;

namespace Platform.Tenants.Services;

/// <summary>Host đăng ký — tạo user TENANT_ADMIN / ORG_ADMIN sau khi tạo catalog.</summary>
public interface IPlatformCatalogAdminProvisioner
{
    Task<CatalogAdminProvisionResult> ProvisionTenantAdminAsync(
        Guid tenantId,
        CatalogAdminAccountInput admin,
        CancellationToken cancellationToken = default);

    Task<CatalogAdminProvisionResult> ProvisionOrgAdminAsync(
        Guid tenantId,
        Guid orgId,
        CatalogAdminAccountInput admin,
        CancellationToken cancellationToken = default);
}
