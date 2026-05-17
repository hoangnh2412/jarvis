using System.Linq.Expressions;
using Jarvis.Domain.Entities;

namespace Jarvis.Domain.Repositories;

/// <summary>
/// Read-only repository; queries use no-tracking by default.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IQueryRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    IQueryable<TEntity> GetQuery();

    Task<ICollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Paged list with optional expression predicate and optional column projection (<see cref="PagedListRequest.Columns"/>).
    /// </summary>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> PaginationAsync(
        PagedListRequest query,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
