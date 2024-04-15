using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Persistence.DataContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sample.DataStorage;

public class TenantConnectionAccessor : ITenantConnectionAccessor
{
    private readonly IConfiguration _configuration;
    private readonly ITenantUnitOfWork _uow;

    public TenantConnectionAccessor(
        IConfiguration configuration,
        ITenantUnitOfWork uow)
    {
        _configuration = configuration;
        _uow = uow;
    }

    public async Task<string> GetConnectionStringAsync(Guid tenantId)
    {
        var repo = _uow.GetRepository<IRepository<Tenant>>();
        var tenant = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == tenantId);
        if (tenant == null)
            throw new Exception($"Not found connection string of tenant {tenantId}");

        return tenant.ConnectionString;
    }
}