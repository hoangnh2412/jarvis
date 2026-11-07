using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.ORM.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class SampleTenantUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<SampleTenantDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor,
    IOrgIdResolverFactory orgIdResolverFactory,
    ICurrentOrgAccessor currentOrgAccessor,
    Lazy<IRequestScopeResolver>? requestScopeResolver = null)
    : BaseUnitOfWork<SampleTenantDbContext>(
        services,
        factory,
        tenantIdResolverFactory,
        currentTenantAccessor,
        orgIdResolverFactory,
        currentOrgAccessor,
        requestScopeResolver), ISampleTenantUnitOfWork
{
}
