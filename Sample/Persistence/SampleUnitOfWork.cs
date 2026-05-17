using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class SampleUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<SampleDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory)
    : BaseUnitOfWork<SampleDbContext>(services, factory, tenantIdResolverFactory), ISampleUnitOfWork
{
}
