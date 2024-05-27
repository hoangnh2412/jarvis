using System.Linq.Expressions;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories.Dapper;

public partial class BaseCommandRepository<TEntity> : ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
    protected DapperDbContext StorageContext;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DapperDbContext;
    }

    public Task<TEntity> InsertAsync(TEntity entity)
    {
        // var result = await DbSet.AddAsync(entity);
        // return result.Entity;

        return Task.FromResult(default(TEntity));
    }

    public Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        // await DbSet.AddRangeAsync(entities);

        return Task.CompletedTask;
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        // var entry = StorageContext.Entry(entity);
        // if (entry.State == EntityState.Detached)
        //     StorageContext.Attach(entity);

        // entry.State = EntityState.Modified;
        // return Task.FromResult(entry.Entity);

        return Task.FromResult(default(TEntity));
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities)
    {
        // foreach (var entity in entities)
        // {
        //     var entry = StorageContext.Entry(entity);
        //     if (entry.State == EntityState.Detached)
        //         StorageContext.Attach(entity);

        //     entry.State = EntityState.Modified;
        // }
        return Task.CompletedTask;
    }

    public Task<TEntity> DeleteAsync(TEntity entity)
    {
        // var entry = StorageContext.Entry(entity);
        // if (entry.State == EntityState.Detached)
        //     StorageContext.Attach(entity);

        // entry.State = EntityState.Deleted;
        // return Task.FromResult(entry.Entity);

        return Task.FromResult(default(TEntity));
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
        // foreach (var entity in entities)
        // {
        //     var entry = StorageContext.Entry(entity);
        //     if (entry.State == EntityState.Detached)
        //         StorageContext.Attach(entity);

        //     entry.State = EntityState.Deleted;
        // }
        return Task.CompletedTask;
    }

    public Task InsertBatchAsync(IEnumerable<TEntity> entities)
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

    public Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBatchAsync(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }
}