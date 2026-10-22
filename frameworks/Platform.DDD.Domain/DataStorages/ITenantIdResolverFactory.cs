namespace Platform.DDD.Domain.DataStorages;

/// <summary>
/// Resolves tenant id using the default chain: header → user claim → query string → host (when parseable as <see cref="Guid"/>).
/// Default implementation: <c>Platform.Multitenancy.DataStorages.TenantIdResolverFactory</c>.
/// </summary>
public interface ITenantIdResolverFactory
{
    Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default);

    /// <summary>Domain (host lookup) → user claim → query — không đọc header <c>X-Tenant-Id</c>.</summary>
    Task<Guid?> GetTenantIdForOrgScopeAsync(CancellationToken cancellationToken = default);
}
