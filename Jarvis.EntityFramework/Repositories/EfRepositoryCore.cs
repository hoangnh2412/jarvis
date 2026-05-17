using System.Linq.Expressions;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.Repositories;

/// <summary>
/// Shared EF Core repository implementation (context, reads with no-tracking, writes).
/// </summary>
public abstract class EfRepositoryCore<TEntity>
    where TEntity : class, IEntity
{
    protected DbContext StorageContext { get; private set; } = null!;
    protected DbSet<TEntity> DbSet { get; private set; } = null!;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = (DbContext)storageContext;
        DbSet = StorageContext.Set<TEntity>();
    }

    public IQueryable<TEntity> GetQuery() => DbSet.AsNoTracking();

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = GetQuery();
        if (predicate == null)
            return await queryable.AnyAsync(cancellationToken).ConfigureAwait(false);

        return await queryable.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = GetQuery();
        if (predicate == null)
            return await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        return await queryable.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ICollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = GetQuery();
        if (predicate == null)
            return await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

        return await queryable.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<(IReadOnlyList<TEntity> Items, int TotalCount)> PaginationAsync(
        PagedListRequest paging,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return PagedListExecutor.ExecuteAsync(GetQuery(), paging, predicate, cancellationToken);
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return result.Entity;
    }

    public async Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.FromResult(entity);
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task<TEntity?> GetByIdAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> LoadAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
