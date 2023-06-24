using System;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;

namespace Jarvis.Core.Abstractions
{
    public interface ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingOutput, TCreateInput, TUpdateInput>
        // where TUnitOfWork : IUnitOfWork
        // where TEntity : class, IEntity<TKey>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        Task<IPaged<TPagingOutput>> PaginationAsync(IPaging paging);

        Task<TModel> GetByKeyAsync(Guid key);

        Task<TModel> GetByIdAsync(TKey id);

        Task<int> CreateAsync(TCreateInput input);

        Task<int> UpdateAsync(Guid key, TUpdateInput input);

        Task<int> UpdateAsync(TKey id, TUpdateInput input);

        Task<int> DeleteAsync(TKey id);

        Task<int> DeleteAsync(Guid key);

        Task<int> TrashAsync(TKey id);

        Task<int> TrashAsync(Guid key);
    }
}