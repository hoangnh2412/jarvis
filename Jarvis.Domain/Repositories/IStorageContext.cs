namespace Jarvis.Domain.Repositories;

/// <summary>
/// The interface abstract storage
/// </summary>
public interface IStorageContext
{
    void SetTenantId(string? tenantId);
}