using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories.EntityFramework;

public class BaseRepository<TEntity> : BaseCommandRepository<TEntity>, IEFRepository<TEntity>
    where TEntity : class, IEntity
{
    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        var queryable = DbSet as IQueryable<TEntity>;
        return asNoTracking ? queryable.AsNoTracking() : queryable;
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