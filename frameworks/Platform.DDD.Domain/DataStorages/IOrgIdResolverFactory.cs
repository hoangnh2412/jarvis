namespace Platform.DDD.Domain.DataStorages;

/// <summary>
/// Default chain: header → user claim → query → host (when parseable as <see cref="Guid"/>).
/// </summary>
public interface IOrgIdResolverFactory
{
    Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default);
}
