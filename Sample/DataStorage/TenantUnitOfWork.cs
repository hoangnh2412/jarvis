using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage;

public class TenantUnitOfWork : BaseEFUnitOfWork<TenantDbContext>, ITenantUnitOfWork
{
    public TenantUnitOfWork(
        IServiceProvider services,
        IDbContextFactory<TenantDbContext> factory) : base(services, factory)
    {
    }
}