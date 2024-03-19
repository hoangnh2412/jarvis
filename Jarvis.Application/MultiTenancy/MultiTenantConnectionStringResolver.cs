namespace Jarvis.Application.MultiTenancy;

public class MultiTenantConnectionStringResolver : IConnectionStringResolver
{
    private readonly ITenantIdAccessor _idAccessor;
    private readonly ITenantConnectionAccessor _connectionAccessor;

    public MultiTenantConnectionStringResolver(
        ITenantIdAccessor idAccessor,
        ITenantConnectionAccessor connectionAccessor)
    {
        _idAccessor = idAccessor;
        _connectionAccessor = connectionAccessor;
    }

    public async Task<string> GetConnectionStringAsync(string name)
    {
        var tenantId = await _idAccessor.GetCurrentAsync();
        return await _connectionAccessor.GetConnectionStringAsync(tenantId);
    }
}