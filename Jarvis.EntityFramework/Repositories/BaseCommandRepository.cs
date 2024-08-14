using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.Repositories;

public class BaseCommandRepository<TEntity> : ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    protected DbContext StorageContext;
    protected DbSet<TEntity> DbSet;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = (DbContext)storageContext;
        DbSet = StorageContext.Set<TEntity>();
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        var result = await DbSet.AddAsync(entity);
        return result.Entity;
    }

    public async Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        var entry = StorageContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            StorageContext.Attach(entity);

        entry.State = EntityState.Modified;
        return Task.FromResult(entry.Entity);
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            entry.State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    public Task<TEntity> DeleteAsync(TEntity entity)
    {
        var entry = StorageContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            StorageContext.Attach(entity);

        entry.State = EntityState.Deleted;
        return Task.FromResult(entry.Entity);
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            entry.State = EntityState.Deleted;
        }
        return Task.CompletedTask;
    }
}