using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.Repositories;

namespace Sample.DataStorage;

public class TenantUnitOfWork : BaseEFUnitOfWork<TenantDbContext>, ITenantUnitOfWork
{
    // public TenantUnitOfWork(
    //     IServiceProvider services,
    //     IDbContextFactory<TenantDbContext> factory) : base(services, factory)
    // {
    // }
    public TenantUnitOfWork(IServiceProvider services, Func<string, IStorageContext> factory) : base(services, factory)
    {
    }
}