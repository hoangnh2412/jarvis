using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Database.Dapper
{
    public class DapperUnitOfWork<T> : IUnitOfWork<T> where T : IStorageContext
    {
        private readonly IServiceProvider _services;
        protected IStorageContext StorageContext { get; private set; }

        public DapperUnitOfWork(
            IServiceProvider services,
            IEnumerable<IStorageContext> storageContexts)
        {
            _services = services;
            StorageContext = storageContexts.FirstOrDefault(x => x.GetType().Name == typeof(T).Name);
        }

        public IStorageContext GetDbContext()
        {
            return StorageContext;
        }

        public string GetConnectionString()
        {
            var dbContext = StorageContext as BaseStorageContext;
            return dbContext.GetConnectionString();
        }

        public TRepository GetRepository<TRepository>() where TRepository : IRepository
        {
            var repo = (TRepository)_services.GetService(typeof(TRepository));
            if (repo != null)
                repo.SetStorageContext(StorageContext);
            return repo;
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException("Dapper ORM is not implement CommitAsync function");
        }

        public TTransaction BeginTransaction<TTransaction>(IsolationLevel? isolation = null)
        {
            throw new NotImplementedException("Dapper ORM is not implement CommitAsync function");
        }

        public async Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel? isolation = null)
        {
            await Task.Yield();
            throw new NotImplementedException("Dapper ORM is not implement BeginTransactionAsync<TTransaction> function");
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException("Dapper ORM is not implement CommitTransaction function");
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException("Dapper ORM is not implement RollbackTransaction function");
        }

        public TTransaction CurrentTransaction<TTransaction>()
        {
            throw new NotImplementedException("Dapper ORM is not implement CurrentTransaction<TTransaction> function");
        }

        public TTransaction UseTransaction<TTransaction>(DbTransaction transaction)
        {
            throw new NotImplementedException("Dapper ORM is not implement UseTransaction<TTransaction> function");
        }
    }
}
