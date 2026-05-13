using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class SampleUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<SampleDbContext> factory,
    IStorageContextTenantInitializer? tenantInitializer = null)
    : BaseUnitOfWork<SampleDbContext>(services, factory, tenantInitializer), ISampleUnitOfWork
{
}