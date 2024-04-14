namespace Jarvis.Application.MultiTenancy;

public interface ITenantConnectionAccessor
{
    Task<string> GetConnectionStringAsync(Guid tenantId);
}