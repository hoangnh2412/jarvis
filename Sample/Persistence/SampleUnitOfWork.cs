using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.Persistence;

public class SampleUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<SampleDbContext> factory)
    : BaseUnitOfWork<SampleDbContext>(services, factory), ISampleUnitOfWork
{
}