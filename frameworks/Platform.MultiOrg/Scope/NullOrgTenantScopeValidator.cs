using Platform.DDD.Domain.Services;

namespace Platform.MultiOrg.Scope;

/// <summary>No-op khi host chưa đăng ký catalog — production dùng <c>OrgCatalogTenantScopeValidator</c>.</summary>
public sealed class NullOrgTenantScopeValidator : IOrgTenantScopeValidator
{
    public Task ValidateOrgBelongsToTenantAsync(
        Guid orgId,
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
