using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories;

public class BaseQueryRepository<TEntity> : IQueryRepository<TEntity>
    where TEntity : class, IEntity
{
    private IQueryable<TEntity> Queryable;
    private DbContext StorageContext;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DbContext;
        Queryable = StorageContext.Set<TEntity>() as IQueryable<TEntity>;
    }

    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        return asNoTracking ? Queryable.AsNoTracking() : Queryable;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.AnyAsync();

        return await queryable.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.CountAsync();

        return await queryable.CountAsync(predicate);
    }

    public async Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        var queryable = GetQuery(asNoTracking);
        if (predicate == null)
            return await queryable.ToListAsync();

        return await queryable.Where(predicate).ToListAsync();
    }
}