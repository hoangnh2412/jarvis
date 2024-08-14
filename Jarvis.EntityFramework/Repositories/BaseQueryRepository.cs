using System.Linq.Expressions;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.Repositories;

public class BaseQueryRepository<TEntity> : IQueryRepository<TEntity>
    where TEntity : class, IEntity
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private IQueryable<TEntity> Queryable;
    private DbContext StorageContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = (DbContext)storageContext;
        Queryable = StorageContext.Set<TEntity>();
    }

    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        return asNoTracking ? Queryable.AsNoTracking() : Queryable;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.AnyAsync();

        return await queryable.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.CountAsync();

        return await queryable.CountAsync(predicate);
    }

    public async Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.ToListAsync();

        return await queryable.Where(predicate).ToListAsync();
    }
}