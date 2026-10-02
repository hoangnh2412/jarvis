using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.ORM.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class TenantUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<TenantDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor)
    : BaseUnitOfWork<TenantDbContext>(services, factory, tenantIdResolverFactory, currentTenantAccessor), ITenantUnitOfWork
{
}
