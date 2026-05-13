namespace Jarvis.Domain.Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IStorageContext GetDbContext();

    Task<IStorageContext> GetDbContextAsync();

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