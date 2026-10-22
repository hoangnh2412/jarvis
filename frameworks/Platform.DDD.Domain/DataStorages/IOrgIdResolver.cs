namespace Platform.DDD.Domain.DataStorages;

/// <summary>
/// Resolves the current working organization id. Implementations live in <c>Platform.MultiOrg</c>.
/// </summary>
public interface IOrgIdResolver
{
    Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default);
}
