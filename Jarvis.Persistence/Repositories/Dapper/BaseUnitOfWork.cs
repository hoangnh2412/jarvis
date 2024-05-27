using System.Data;
using System.Data.Common;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.DependencyInjection;

namespace Jarvis.Persistence.Repositories.Dapper;

public abstract class BaseUnitOfWork<T> : IUnitOfWork<T> where T : IStorageContext
{
    private readonly IServiceProvider _services;
    protected DapperDbContext StorageContext { get; }

    protected BaseUnitOfWork(
        IServiceProvider services,
        IStorageContext storageContext)
    {
        _services = services;
        StorageContext = storageContext as DapperDbContext;
    }

    public IStorageContext GetDbContext()
    {
        return StorageContext;
    }

    public IStorageContext GetDbContext(string name)
    {
        var resolverName = InstanceStorage.DbContexts[StorageContext.GetType().AssemblyQualifiedName];
        var resolver = (ITenantConnectionStringResolver)_services.GetService(Type.GetType(resolverName));

        var connectionString = resolver.GetConnectionString(name);
        StorageContext.SetConnectionString(connectionString);
        return GetDbContext();
    }

    public IStorageContext GetDbContext<TResolver>(string name)
    {
        var resolver = _services.GetService<ITenantConnectionStringResolver>(typeof(TResolver).Name);

        var connectionString = resolver.GetConnectionString(name);
        StorageContext.SetConnectionString(connectionString);
        return GetDbContext();
    }

    public string GetConnectionString()
    {
        return StorageContext.GetConnectionString();
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        // var repo = (TRepository)_services.GetService(typeof(TRepository));

        var type = typeof(TRepository);
        var repoObj = _services.GetService(type);

        var repo = (TRepository)repoObj;
        if (repo != null)
            repo.SetStorageContext(StorageContext);
        return repo;
    }

    public TTransaction BeginTransaction<TTransaction>(IsolationLevel? isolation = null)
    {
        throw new NotImplementedException();
    }

    public Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel? isolation = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> CommitAsync()
    {
        throw new NotImplementedException();
    }

    public void CommitTransaction()
    {
        throw new NotImplementedException();
    }

    public TTransaction CurrentTransaction<TTransaction>()
    {
        throw new NotImplementedException();
    }

    public void RollbackTransaction()
    {
        throw new NotImplementedException();
    }

    public TTransaction UseTransaction<TTransaction>(DbTransaction transaction)
    {
        throw new NotImplementedException();
    }
}