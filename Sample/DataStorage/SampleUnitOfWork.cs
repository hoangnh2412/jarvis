using Jarvis.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage;

public class SampleUnitOfWork : BaseEFUnitOfWork<SampleDbContext>, ISampleUnitOfWork
{
    public SampleUnitOfWork(
        IServiceProvider services,
        IDbContextFactory<SampleDbContext> factory) : base(services, factory)
    {
    }
}