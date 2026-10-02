using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.ORM.EntityFramework.Repositories;
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
