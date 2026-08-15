namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// API public cho tenant đang làm việc: id (R2) và profile (store).
/// </summary>
public interface ICurrentTenant<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    /// <summary>
    /// Ambient <see cref="ICurrentTenantAccessor.TenantId"/>, không có thì
    /// <see cref="DataStorages.ITenantIdResolverFactory"/> (R2; cache trên scoped instance).
    /// Không load store.
    /// </summary>
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Profile: <see cref="GetIdAsync"/> → <see cref="ICurrentTenantStore{TTenant}"/> (cache scoped theo id).
    /// Không có profile trong store thì trả <c>null</c> — không fallback.
    /// </summary>
    Task<TTenant?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gán ambient tenant đến khi dispose (bọc <see cref="ICurrentTenantAccessor.BeginScope"/>).
    /// </summary>
    IDisposable Change(Guid tenantId);
}
