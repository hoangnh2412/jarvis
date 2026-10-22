using Platform.DDD.Domain.Services;
using Platform.Orgs.Abstractions;

namespace Platform.Orgs.Services;

public sealed class OrgCatalogTenantScopeValidator(IOrgCatalog orgCatalog) : IOrgTenantScopeValidator
{
    public Task ValidateOrgBelongsToTenantAsync(
        Guid orgId,
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        orgCatalog.ValidateForAssignmentAsync(orgId, tenantId, cancellationToken);
}
