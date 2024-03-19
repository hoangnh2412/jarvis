using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Jarvis.Domain.Common;
using Jarvis.Domain.Common.Interfaces;
using Jarvis.Shared.Constants;

namespace Jarvis.Application.Extensions;

/// <summary>
/// Provides extension functions for IQueryable
/// </summary>
public static partial class QueryableExtension
{
    /// <summary>
    /// Build expression to query Contains by property Name and Value is string
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Contains<T>(this IQueryable<T> queryable, string propertyName, string propertyValue)
    {
        var parameterExp = Expression.Parameter(typeof(T));
        var propertyExp = Expression.Property(parameterExp, propertyName);

        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var constantExpression = Expression.Constant(propertyValue, typeof(string));

        var containsCall = Expression.Call(propertyExp, containsMethod, constantExpression);

        var expression = Expression.Lambda<Func<T, bool>>(containsCall, parameterExp);
        return queryable.Where(expression);
    }

    public static IQueryable<T> QueryByTenantId<T>(this IQueryable<T> queryable, Guid tenantId)
    {
        if (queryable.GetType().GenericTypeArguments[0].GetInterface(nameof(ITenantEntity)) != null)
            queryable = queryable.QueryEqual(nameof(ITenantEntity.TenantId), tenantId);
        return queryable;
    }

    public static IQueryable<T> QueryById<T, TKey>(this IQueryable<T> queryable, TKey id) where T : IEntity<TKey>
    {
        queryable = queryable.Where(x => x.Id.Equals(id));
        return queryable;
    }

    /// <summary>
    /// Build expression to paging.
    /// Nax Size is 100
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="paging"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<IPaged<T>> ToPagedAsync<T>(this IQueryable<T> queryable, IPaging paging)
    {
        var paged = new Paged<T>();
        paged.Size = paging.Size < 0 || paging.Size > QueryableConstant.Pagination.MaxSize ? QueryableConstant.Pagination.MaxSize : paging.Size;
        paged.Page = paging.Page;
        paged.Query = paging.Q;
        paged.TotalItems = await queryable.CountAsync();

        paged.TotalPages = paged.TotalItems / paged.Size;

        if (paged.TotalPages < paged.Page)
            paged.TotalPages = 1;

        if (paged.TotalItems % paged.Size > 0)
            paged.TotalPages++;

        paged.Data = await queryable.Skip((paged.Page - 1) * paged.Size).Take(paged.Size).AsQueryable().ToListAsync();
        return paged;
    }

    /// <summary>
    /// Build expression to query a propertyName use equal operation
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> QueryEqual<T>(this IQueryable<T> queryable, string propertyName, object propertyValue)
    {
        var parameterExp = Expression.Parameter(typeof(T));
        var propertyExp = Expression.PropertyOrField(parameterExp, propertyName);
        var query = Expression.Equal(propertyExp, Expression.Constant(propertyValue));
        var expression = Expression.Lambda<Func<T, bool>>(query, parameterExp);
        return queryable.Where(expression);
    }

    /// <summary>
    /// Build expression to Select the columns by column name
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="columns"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Select<T>(this IQueryable<T> queryable, string[] columns)
    {
        var sourceType = queryable.ElementType;
        var resultType = typeof(T);
        var parameter = Expression.Parameter(sourceType, "e");
        var bindings = columns.Select(column => Expression.Bind(resultType.GetProperty(column), Expression.PropertyOrField(parameter, column)));
        var body = Expression.MemberInit(Expression.New(resultType), bindings);
        var selector = Expression.Lambda(body, parameter);
        var query = queryable.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, resultType }, queryable.Expression, Expression.Quote(selector)));
        return query;
    }

    /// <summary>
    /// Build expression to sort by multi-columns. Priority is sorted according to the order of the sorts variable
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="sorts"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, Dictionary<string, string> sorts)
    {
        var expression = queryable.Expression;
        int count = 0;
        foreach (var item in sorts)
        {
            var parameter = Expression.Parameter(typeof(T));
            var selector = Expression.PropertyOrField(parameter, item.Key);
            var method = string.Equals(item.Value, "desc", StringComparison.OrdinalIgnoreCase) ? (count == 0 ? "OrderByDescending" : "ThenByDescending") : (count == 0 ? "OrderBy" : "ThenBy");
            expression = Expression.Call(typeof(Queryable), method,
                new Type[] { queryable.ElementType, selector.Type },
                expression, Expression.Quote(Expression.Lambda(selector, parameter)));
            count++;
        }
        return count > 0 ? queryable.Provider.CreateQuery<T>(expression) : queryable;
    }
}