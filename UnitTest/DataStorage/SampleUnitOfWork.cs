using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.Repositories;

namespace UnitTest.DataStorage;

public class SampleUnitOfWork : BaseEFUnitOfWork<SampleDbContext>, ISampleUnitOfWork
{
    // public SampleUnitOfWork(
    //     IServiceProvider services,
    //     IDbContextFactory<SampleDbContext> factory) : base(services, factory)
    // {
    // }
    public SampleUnitOfWork(IServiceProvider services, Func<string, IStorageContext> factory) : base(services, factory)
    {
    }
}