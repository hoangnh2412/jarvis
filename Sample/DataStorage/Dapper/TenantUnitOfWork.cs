using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.Repositories.Dapper;

namespace Sample.DataStorage.Dapper;

public class TenantUnitOfWork : BaseUnitOfWork<TenantDbContext>, ITenantUnitOfWork
{
    public TenantUnitOfWork(
        IServiceProvider services,
        IStorageContext storageContext) : base(services, storageContext)
    {
    }
}