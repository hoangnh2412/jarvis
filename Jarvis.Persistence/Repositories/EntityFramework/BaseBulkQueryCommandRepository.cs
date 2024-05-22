using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;
using System.Linq.Expressions;

namespace Jarvis.Persistence.Repositories.EntityFramework;

public partial class BaseCommandRepository<TEntity> : ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
    public async Task<int> UpdateBatchAsync(IQueryable<TEntity> queryable, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return await queryable.UpdateFromQueryAsync(updateFactory);
    }

    public async Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable)
    {
        return await BatchDeleteExtensions.DeleteFromQueryAsync(queryable);
    }
}