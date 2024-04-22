using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Persistence.Repositories;

public abstract class BaseEFUnitOfWork<T> : IUnitOfWork<T> where T : DbContext, IStorageContext
{
    private readonly IServiceProvider _services;
    protected DbContext StorageContext { get; set; }

    public BaseEFUnitOfWork(
        IServiceProvider services,
        IDbContextFactory<T> factory)
    {
        _services = services;
        StorageContext = factory.CreateDbContext();
    }

    public IStorageContext GetDbContext()
    {
        return (IStorageContext)StorageContext;
    }

    public IStorageContext GetDbContext(string name)
    {
        var resolver = _services.GetService<ITenantConnectionStringResolver>();

        var connectionString = resolver.GetConnectionString(name);
        StorageContext.Database.SetConnectionString(connectionString);
        return GetDbContext();
    }

    public IStorageContext GetDbContext<TResolver>(string name)
    {
        var factory = _services.GetService<Func<string, ITenantConnectionStringResolver>>();
        var resolver = factory.Invoke(typeof(TResolver).AssemblyQualifiedName);

        var connectionString = resolver.GetConnectionString(name);
        StorageContext.Database.SetConnectionString(connectionString);
        return GetDbContext();
    }

    public string GetConnectionString()
    {
        var dbContext = StorageContext as DbContext;
        return dbContext.Database.GetDbConnection().ConnectionString;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        var repo = (TRepository)_services.GetService(typeof(TRepository));
        if (repo != null)
            repo.SetStorageContext((IStorageContext)StorageContext);
        return repo;
    }

    public async Task<int> CommitAsync()
    {
        return await StorageContext.SaveChangesAsync();
    }

    public TTransaction BeginTransaction<TTransaction>(IsolationLevel? isolation = null)
    {
        if (isolation.HasValue)
            return (TTransaction)StorageContext.Database.BeginTransaction(isolation.Value);

        return (TTransaction)StorageContext.Database.BeginTransaction();
    }

    public async Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel? isolation = null)
    {
        if (isolation.HasValue)
            return (TTransaction)(await StorageContext.Database.BeginTransactionAsync(isolation.Value));

        return (TTransaction)(await StorageContext.Database.BeginTransactionAsync());
    }

    public void CommitTransaction()
    {
        StorageContext.Database.CommitTransaction();
    }

    public void RollbackTransaction()
    {
        StorageContext.Database.RollbackTransaction();
    }

    public TTransaction CurrentTransaction<TTransaction>()
    {
        return (TTransaction)StorageContext.Database.CurrentTransaction;
    }

    public TTransaction UseTransaction<TTransaction>(DbTransaction transaction)
    {
        return (TTransaction)StorageContext.Database.UseTransaction(((IDbContextTransaction)transaction).GetDbTransaction());
    }
}