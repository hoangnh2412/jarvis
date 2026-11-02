using Platform.Modules.Account.Abstractions;
using Platform.Orgs.Abstractions;
using Platform.Tenants.Services;

namespace Platform.Modules.Account.Services;

/// <summary>Danh mục tenant/org đang active cho màn đăng nhập app (chỉ Id, Code, Name).</summary>
public sealed class PlatformLoginCatalogService(
    ITenantManager tenants,
    IOrgCatalog orgs) : IPlatformLoginCatalogService
{
    public async Task<IReadOnlyList<LoginCatalogItem>> ListActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await tenants.ListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return rows
            .Where(row => row.IsActive)
            .OrderBy(row => row.Code, StringComparer.Ordinal)
            .Select(row => new LoginCatalogItem(row.Id, row.Code, row.Name))
            .ToArray();
    }

    public async Task<IReadOnlyList<LoginCatalogItem>?> ListActiveOrgsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null || !tenant.IsActive)
            return null;

        var rows = await orgs.ListAsync(tenantId, isActive: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        return rows
            .OrderBy(row => row.Code, StringComparer.Ordinal)
            .Select(row => new LoginCatalogItem(row.Id, row.Code, row.Name))
            .ToArray();
    }
}
