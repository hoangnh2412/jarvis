using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class SampleUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<TenantDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor)
    : BaseUnitOfWork<TenantDbContext>(services, factory, tenantIdResolverFactory, currentTenantAccessor), ISampleUnitOfWork
{
}
