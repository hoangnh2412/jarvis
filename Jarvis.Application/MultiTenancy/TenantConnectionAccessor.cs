namespace Jarvis.Application.MultiTenancy;
public class TenantConnectionAccessor : ITenantConnectionAccessor
{
    public Task<string> GetConnectionStringAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetConnectionStringAsync(Guid tenantId)
    {
        throw new NotImplementedException();
    }
}