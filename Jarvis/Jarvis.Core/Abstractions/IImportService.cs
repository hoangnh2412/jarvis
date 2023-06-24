using System.Threading.Tasks;

namespace Jarvis.Core.Abstractions
{
    public interface IImportService<TUnitOfWork, TKey, TEntity, TModel, TImportInput>
        // where TUnitOfWork : IUnitOfWork
        // where TEntity : class, IEntity<TKey>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        Task<int> ImportAsync(byte[] bytes, bool useBulk = false);
    }
}