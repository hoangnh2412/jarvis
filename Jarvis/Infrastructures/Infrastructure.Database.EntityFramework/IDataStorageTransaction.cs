using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Database.EntityFramework
{
    public interface IDataStorageTransaction
    {
        IDbContextTransaction BeginTransaction(IsolationLevel? isolation = null);

        Task<IDbContextTransaction> BeginTransactionAsync();

        void CommitTransaction();

        void RollbackTransaction();

        IDbContextTransaction CurrentTransaction();

        IDbContextTransaction UseTransaction(IDbContextTransaction transaction);
    }
}
