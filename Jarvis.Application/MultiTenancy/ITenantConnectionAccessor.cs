namespace Jarvis.Application.MultiTenancy;

public interface ITenantConnectionAccessor
{
    Task<string> GetConnectionStringAsync();

    Task<string> GetConnectionStringAsync(Guid tenantId);
}