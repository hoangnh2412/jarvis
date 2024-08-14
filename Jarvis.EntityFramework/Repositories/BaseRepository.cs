using System.Linq.Expressions;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.Repositories;

public class BaseRepository<TEntity> : BaseCommandRepository<TEntity>, IRepository<TEntity>
    where TEntity : class, IEntity
{
    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        var queryable = DbSet as IQueryable<TEntity>;
        return asNoTracking ? queryable.AsNoTracking() : queryable;
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