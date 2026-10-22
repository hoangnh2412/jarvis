namespace Platform.DDD.Domain.Services;

/// <summary>
/// API public cho working organization: id (R2) và profile (store).
/// </summary>
public interface ICurrentOrg<TOrg>
    where TOrg : class, ICurrentOrgIdentity
{
    /// <summary>
    /// Ambient <see cref="ICurrentOrgAccessor.OrgId"/>, không có thì
    /// <see cref="DataStorages.IOrgIdResolverFactory"/> (R2; cache trên scoped instance).
    /// </summary>
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Profile: <see cref="GetIdAsync"/> → <see cref="ICurrentOrgStore{TOrg}"/>.
    /// </summary>
    Task<TOrg?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gán ambient org đến khi dispose (bọc <see cref="ICurrentOrgAccessor.BeginScope"/>).
    /// </summary>
    IDisposable Change(Guid orgId);
}
