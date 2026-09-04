namespace Jarvis.DDD.Domain.DataStorages;

/// <summary>
/// Resolves tenant id using the default chain: header → user claim → query string → host (when parseable as <see cref="Guid"/>).
/// Default implementation: <c>Jarvis.Multitenancy.DataStorages.TenantIdResolverFactory</c>.
/// </summary>
public interface ITenantIdResolverFactory
{
    Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}
