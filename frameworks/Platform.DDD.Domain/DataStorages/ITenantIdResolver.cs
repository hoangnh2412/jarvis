namespace Platform.DDD.Domain.DataStorages;

/// <summary>
/// Resolves the current tenant id (<see cref="Guid"/>).
/// Implementations (header / claim / query / host) live in <c>Platform.Multitenancy</c>;
/// register via <c>AddCurrentTenant</c> / <c>AddTenantIdResolvers</c>.
/// </summary>
public interface ITenantIdResolver
{
    Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}
