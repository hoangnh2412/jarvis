using System.Linq.Expressions;
using Jarvis.DDD.Domain.Entities;

namespace Jarvis.DDD.Domain.Repositories;

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
    /// Paged list with advanced filtering, sorting and custom options.
    /// Static scope: apply <c>Where</c> on <see cref="GetQuery"/> before calling, or use <see cref="PagedQueryOptions{TEntity}.CustomFilter"/>.
    /// </summary>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> PaginationAsync(
        PagedListRequest query,
        PagedQueryOptions<TEntity>? options,
        CancellationToken cancellationToken = default);
}
