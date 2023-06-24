using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Infrastructure.Database.Abstractions
{
    public interface IUnitOfWork
    {
        IStorageContext GetDbContext();

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

    public interface IUnitOfWork<TStorageContext> : IUnitOfWork where TStorageContext : IStorageContext
    {

    }
}
