using Jarvis.Domain.DataStorages;

namespace Jarvis.Domain.Repositories;

public interface IUnitOfWork
{
    IStorageContext GetDbContext();

    IStorageContext GetDbContext<TResolver>(string name) where TResolver : ITenantConnectionStringResolver;

    string GetConnectionString();

    TRepository GetRepository<TRepository>() where TRepository : IRepository;

    Task<int> CommitAsync();
}

/// <summary>
/// The interface abstract communication actions with multiple entities and specific StorageContext
/// </summary>
/// <typeparam name="TStorageContext"></typeparam>
public interface IUnitOfWork<TStorageContext> : IUnitOfWork where TStorageContext : IStorageContext
{

}