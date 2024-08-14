using System.Data;

namespace Jarvis.Domain.Repositories;

public interface IUnitOfWorkTransaction
{
    IStorageContext GetDbContext();
 
    IStorageContext GetDbContext(string name);

    IStorageContext GetDbContext<TResolver>(string name);

    string GetConnectionString();

    TRepository GetRepository<TRepository>() where TRepository : IRepository;

    Task<int> CommitAsync();

    TTransaction BeginTransaction<TTransaction>(IsolationLevel? isolation = null);

    Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel? isolation = null);

    void CommitTransaction();

    void RollbackTransaction();

    TTransaction CurrentTransaction<TTransaction>();

    TTransaction UseTransaction<TTransaction>(IDbTransaction transaction);
}

/// <summary>
/// The interface abstract communication actions with multiple entities and specific StorageContext
/// </summary>
/// <typeparam name="TStorageContext"></typeparam>
public interface IUnitOfWorkTransaction<TStorageContext> : IUnitOfWork where TStorageContext : IStorageContext
{

}