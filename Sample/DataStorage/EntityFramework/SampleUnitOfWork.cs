using Jarvis.Persistence.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage.EntityFramework;

public class SampleUnitOfWork : BaseUnitOfWork<SampleDbContext>, ISampleUnitOfWork
{
    public SampleUnitOfWork(
        IServiceProvider services,
        IDbContextFactory<SampleDbContext> factory) : base(services, factory)
    {
    }
}