using System.Data;
using System.Data.Common;

namespace Jarvis.Application.Interfaces.Repositories;

/// <summary>
/// The interface abstract communication actions with multiple entities
/// </summary>
public interface IUnitOfWork
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

    TTransaction UseTransaction<TTransaction>(DbTransaction transaction);
}

/// <summary>
/// The interface abstract communication actions with multiple entities and specific StorageContext
/// </summary>
/// <typeparam name="TStorageContext"></typeparam>
public interface IUnitOfWork<TStorageContext> : IUnitOfWork where TStorageContext : IStorageContext
{

}