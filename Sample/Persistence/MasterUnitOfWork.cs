using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class MasterUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<MasterDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor)
    : BaseUnitOfWork<MasterDbContext>(services, factory, tenantIdResolverFactory, currentTenantAccessor), IMasterUnitOfWork
{
}
