using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;

namespace Sample.DataStorage.EntityFramework;

public class StorageConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly ITenantUnitOfWork _uow;

    public StorageConnectionStringResolver(
        ITenantUnitOfWork uow)
    {
        _uow = uow;
    }

    public string GetConnectionString(string tenantIdOrName = null)
    {
        Guid tenantId = GetTenantId(tenantIdOrName);

        var repo = _uow.GetRepository<IRepository<Tenant>>();
        var tenant = repo.GetQuery().FirstOrDefault(x => x.Id == tenantId);
        if (tenant == null)
            throw new Exception($"Connection string of tenant {tenantId} not found");

        return tenant.ConnectionString;
    }

    private Guid GetTenantId(string tenantIdOrName)
    {
        var repo = _uow.GetRepository<IRepository<Tenant>>();
        var tenant = repo.GetQuery().FirstOrDefault(x => x.Name == tenantIdOrName);
        if (tenant == null)
            return Guid.Empty;

        return tenant.Id;
    }
}