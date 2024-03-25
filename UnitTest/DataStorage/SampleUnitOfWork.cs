using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.Repositories;

namespace UnitTest.DataStorage;

public class SampleUnitOfWork : BaseUnitOfWork<SampleDbContext>, ISampleUnitOfWork
{
    public SampleUnitOfWork(IServiceProvider services, Func<string, IStorageContext> factory) : base(services, factory)
    {
    }
}