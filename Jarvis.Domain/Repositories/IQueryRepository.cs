using System.Linq.Expressions;
using Jarvis.Domain.Entities;

namespace Jarvis.Domain.Repositories;

/// <summary>
/// The interface abstracts method to handle read-only operation
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IQueryRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    IQueryable<TEntity> GetQuery(bool asNoTracking = false);

    Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false);

    /// <summary>
    /// Paged list with optional expression predicate and optional column projection (<see cref="PagedListRequest.Columns"/>).
    /// </summary>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> PaginationAsync(
        PagedListRequest query,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);
}