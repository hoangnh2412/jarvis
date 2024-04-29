using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Application.Interfaces.Repositories;

namespace Jarvis.Persistence.Repositories;

public abstract class BaseDapperUnitOfWork<T> : IUnitOfWork<T> where T : IStorageContext
{
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

    public string GetConnectionString()
    {
        throw new NotImplementedException();
    }

    public IStorageContext GetDbContext()
    {
        throw new NotImplementedException();
    }

    public IStorageContext GetDbContext(string name)
    {
        throw new NotImplementedException();
    }

    public IStorageContext GetDbContext<TResolver>(string name)
    {
        throw new NotImplementedException();
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
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