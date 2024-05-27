using System.Data;
using System.Linq.Expressions;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories.Dapper;

public class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected DapperDbContext StorageContext;
    private IDbConnection _connection;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DapperDbContext;
        _connection = StorageContext.Connection;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> DeleteAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBatchAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> InsertAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task InsertBatchAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateBatchAsync(IQueryable<TEntity> queryable, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        throw new NotImplementedException();
    }

    public Task UpdateBatchAsync(IEnumerable<TEntity> entities, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        throw new NotImplementedException();
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }
}