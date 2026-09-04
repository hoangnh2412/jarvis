namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Load full profile tenant (DB/cache). Host bắt buộc đăng ký implementation thật.
/// </summary>
public interface ICurrentTenantStore<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    Task<TTenant?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
