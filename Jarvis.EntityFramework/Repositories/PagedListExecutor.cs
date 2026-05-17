using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jarvis.EntityFramework.Repositories;

internal static class PagedListExecutor
{
    public static async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> ExecuteAsync<TEntity>(
        IQueryable<TEntity> rootQuery,
        PagedListRequest paging,
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        if (paging.PageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(paging), "PageSize must be positive.");
        if (paging.PageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(paging), "PageIndex must be non-negative.");

        var q = rootQuery;
        if (predicate != null)
            q = q.Where(predicate);

        var total = await q.CountAsync(cancellationToken).ConfigureAwait(false);
        q = q.ApplyColumnSelection(paging.Columns);
        var items = await q
            .Skip(paging.PageIndex * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return (items, total);
    }
}
