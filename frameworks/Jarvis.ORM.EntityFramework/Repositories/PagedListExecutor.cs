// Coordinates the dynamic query pipeline: Filter -> Count -> Sort -> Columns -> Pagination.
// Static scope (RLS, soft-delete, …) belongs on the incoming IQueryable before ExecuteAsync.
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.DDD.Domain.Querying;
using Jarvis.ORM.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.ORM.EntityFramework.Repositories;

public static class PagedListExecutor
{
    public static async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> ExecuteAsync<TEntity>(
        IQueryable<TEntity> rootQuery,
        PagedListRequest paging,
        PagedQueryOptions<TEntity>? options,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        if (paging.Size <= 0)
            throw new ArgumentOutOfRangeException(nameof(paging), "Size must be positive.");
        // if (paging.Size > 1000)
        //     throw new ArgumentOutOfRangeException(nameof(paging), "Size cannot exceed 1000.");
        if (paging.Page < 1)
            throw new ArgumentOutOfRangeException(nameof(paging), "Page must be greater than or equal to 1.");

        options ??= new PagedQueryOptions<TEntity>();
        var q = rootQuery;

        // 1. Dynamic Filter (or Custom Override)
        if (options.CustomFilter != null)
            q = options.CustomFilter(q);
        else if (!string.IsNullOrWhiteSpace(paging.Filter))
        {
            var filterAst = FilterParser.Parse(paging.Filter);
            q = q.ApplyDynamicFilter(filterAst, options.AllowedFields, options.DeniedFields);
        }

        // 2. Count Total after filters
        var total = await q.CountAsync(cancellationToken).ConfigureAwait(false);

        // 3. Sort (or Custom Override)
        IOrderedQueryable<TEntity> orderedQuery;
        if (options.CustomSort != null)
        {
            orderedQuery = options.CustomSort(q);
        }
        else
        {
            var sortFields = SortParser.Parse(paging.Sort);
            orderedQuery = q.ApplyDynamicSort(sortFields, options.AllowedFields, options.DeniedFields);
        }

        // 4. Column Projection
        var projectedQuery = orderedQuery.ApplyColumnSelection(paging.Columns, options.AllowedFields, options.DeniedFields);

        // 5. Pagination
        var items = await projectedQuery
            .Skip((paging.Page - 1) * paging.Size)
            .Take(paging.Size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (items, total);
    }
}
