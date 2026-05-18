using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class MasterUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<MasterDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory)
    : BaseUnitOfWork<MasterDbContext>(services, factory, tenantIdResolverFactory), IMasterUnitOfWork
{
}
