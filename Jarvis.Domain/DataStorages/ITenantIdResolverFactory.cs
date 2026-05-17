namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Resolves tenant id using the default chain: header → user claim → query string → host (when parseable as <see cref="Guid"/>).
/// </summary>
public interface ITenantIdResolverFactory
{
    Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}
