namespace Jarvis.Domain.Repositories;

/// <summary>
/// The interface abstract storage
/// </summary>
public interface IStorageContext
{
    Guid? TenantId { get; }

    void SetTenantId(Guid? tenantId);
}
