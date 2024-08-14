using Jarvis.Persistence.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage.EntityFramework;

public class TenantUnitOfWork : BaseUnitOfWork<TenantDbContext>, ITenantUnitOfWork
{
    public TenantUnitOfWork(
        IServiceProvider services,
        IDbContextFactory<TenantDbContext> factory) : base(services, factory)
    {
    }
}