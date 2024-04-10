using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Domain.Entities;
using Jarvis.Persistence.DataContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Jarvis.Persistence.MultiTenancy;

public class TenantConnectionAccessor : ITenantConnectionAccessor
{
    private readonly IConfiguration _configuration;
    // private readonly ICoreUnitOfWork _uow;

    public TenantConnectionAccessor(
        IConfiguration configuration)
    // ICoreUnitOfWork uow)
    {
        _configuration = configuration;
        // _uow = uow;
    }

    public Task<string> GetConnectionStringAsync()
    {
        return Task.FromResult(_configuration.GetConnectionString("Default"));
    }

    public async Task<string> GetConnectionStringAsync(Guid tenantId)
    {
        // var repo = _uow.GetRepository<IRepository<ITenant>>();
        // var tenant = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == tenantId);
        // if (tenant == null)
        //     throw new Exception($"Not found connection string of tenant {tenantId}");

        // return tenant.ConnectionString;
        return null;
    }
}