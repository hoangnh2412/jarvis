using Microsoft.EntityFrameworkCore;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories;

public class BaseCommandRepository<TEntity> : ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
    protected DbContext StorageContext;
    protected DbSet<TEntity> DbSet;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DbContext;
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