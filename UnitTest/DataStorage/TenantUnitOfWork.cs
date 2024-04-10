using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.Repositories;

namespace UnitTest.DataStorage;

public class TenantUnitOfWork : BaseUnitOfWork<SampleDbContext>, ITenantUnitOfWork
{
    public TenantUnitOfWork(IServiceProvider services, Func<string, IStorageContext> factory) : base(services, factory)
    {
    }
}