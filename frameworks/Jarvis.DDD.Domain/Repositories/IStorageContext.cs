namespace Jarvis.DDD.Domain.Repositories;

/// <summary>
/// The interface abstract storage
/// </summary>
public interface IStorageContext
{
    /// <summary>
    /// Sets the EF query-filter snapshot for tenant-scoped entities.
    /// Working tenant for app code is <c>ICurrentTenant</c> — do not expose a public getter here.
    /// </summary>
    void SetTenantId(Guid? tenantId);
}
