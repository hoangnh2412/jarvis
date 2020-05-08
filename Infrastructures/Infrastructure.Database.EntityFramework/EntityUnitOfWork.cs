using Infrastructure.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;
using System.Data.Common;

namespace Infrastructure.Database.EntityFramework
{
    public class EntityUnitOfWork<T> : IUnitOfWork<T> where T : IStorageContext
    {
        private readonly IServiceProvider _services;
        protected IStorageContext StorageContext { get; private set; }

        public EntityUnitOfWork(
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
            var dbContext = StorageContext as DbContext;
            return dbContext.Database.GetDbConnection().ConnectionString;
        }

        public TRepository GetRepository<TRepository>() where TRepository : IRepository
        {
            var repo = (TRepository)_services.GetService(typeof(TRepository));
            if (repo != null)
                repo.SetStorageContext(StorageContext);
            return repo;
        }

        public async Task CommitAsync()
        {
            var dbContext = StorageContext as DbContext;
            await dbContext.SaveChangesAsync();
        }

        public TTransaction BeginTransaction<TTransaction>(IsolationLevel? isolation = null)
        {
            var dbContext = StorageContext as DbContext;
            if (isolation.HasValue)
                return (TTransaction)dbContext.Database.BeginTransaction(isolation.Value);

            return (TTransaction)dbContext.Database.BeginTransaction();
        }

        public async Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel? isolation = null)
        {
            var dbContext = StorageContext as DbContext;
            if (isolation.HasValue)
                return (TTransaction)(await dbContext.Database.BeginTransactionAsync(isolation.Value));

            return (TTransaction)(await dbContext.Database.BeginTransactionAsync());
        }

        public void CommitTransaction()
        {
            var dbContext = StorageContext as DbContext;
            dbContext.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            var dbContext = StorageContext as DbContext;
            dbContext.Database.RollbackTransaction();
        }

        public TTransaction CurrentTransaction<TTransaction>()
        {
            var dbContext = StorageContext as DbContext;
            return (TTransaction)dbContext.Database.CurrentTransaction;
        }

        public TTransaction UseTransaction<TTransaction>(DbTransaction transaction)
        {
            var dbContext = StorageContext as DbContext;
            return (TTransaction)dbContext.Database.UseTransaction(((IDbContextTransaction)transaction).GetDbTransaction());
        }













        // public IDbContextTransaction BeginTransaction(IsolationLevel? isolation = null)
        // {
        //     var dbContext = StorageContext as DbContext;
        //     if (isolation.HasValue)
        //         return dbContext.Database.BeginTransaction(isolation.Value);

        //     return dbContext.Database.BeginTransaction();
        // }

        // public Task<IDbContextTransaction> BeginTransactionAsync()
        // {
        //     var dbContext = StorageContext as DbContext;
        //     return dbContext.Database.BeginTransactionAsync();
        // }

        // public void CommitTransaction()
        // {
        //     var dbContext = StorageContext as DbContext;
        //     dbContext.Database.CommitTransaction();
        // }

        // public void RollbackTransaction()
        // {
        //     var dbContext = StorageContext as DbContext;
        //     dbContext.Database.RollbackTransaction();
        // }

        // public IDbContextTransaction CurrentTransaction()
        // {
        //     var dbContext = StorageContext as DbContext;
        //     return dbContext.Database.CurrentTransaction;
        // }

        // public IDbContextTransaction UseTransaction(IDbContextTransaction transaction)
        // {
        //     var dbContext = StorageContext as DbContext;
        //     return dbContext.Database.UseTransaction(transaction.GetDbTransaction());
        // }
    }
}
