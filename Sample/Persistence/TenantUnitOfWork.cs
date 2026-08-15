using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Services;
using Jarvis.ORM.EntityFramework.Repositories;
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
