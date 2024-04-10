using Jarvis.Application.MultiTenancy;

namespace Jarvis.Persistence.MultiTenancy;


// Multitenant: Xác định tenant/connection string qua TenantId
// Xác định TenantId gồm 2 nhóm:
// A. HTTP request, gồm 3 cách
// - Header key = X-Tenant-Id
// - Params key = tenantId
// - Domain
// B. Worker (http request = null)
// - Truyền vào theo tham số
// => Tổng 4 cách
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