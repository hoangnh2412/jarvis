using Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastructure.Database.EntityFramework
{
    public static class QueryableExtension
    {
        public static IQueryable<T> Contains<T>(this IQueryable<T> query, string propertyName, string propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(T));
            var propertyExp = Expression.Property(parameterExp, propertyName);

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constantExpression = Expression.Constant(propertyValue, typeof(string));

            var containsCall = Expression.Call(propertyExp, containsMethod, constantExpression);

            var expression = Expression.Lambda<Func<T, bool>>(containsCall, parameterExp);
            return query.Where(expression);
        }

        //public static IQueryable<T> Contains<T>(this IQueryable<T> source, Dictionary<string, string> items)
        //{
        //    var type = Expression.Parameter(typeof(T));

        //    foreach (var item in items)
        //    {
        //        var propertyExp = Expression.Property(type, item.Key);

        //        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        //        var constantExpression = Expression.Constant(item.Value, typeof(string));

        //        var containsCall = Expression.Call(propertyExp, containsMethod, constantExpression);
        //        var expression = Expression.Lambda<Func<T, bool>>(containsCall, type);
        //    }

        //    var query = source.Where(expression);
        //    return query;
        //}

        public static async Task<Paged<T>> ToPaginationAsync<T>(this IQueryable<T> query, IPaging paging)
        {
            var paged = new Paged<T>();
            paged.Q = paging.Q;
            paged.TotalItems = await query.CountAsync();

            if (paging.Size == -1)
            {
                paged.Data = await query.ToListAsync();
            }
            else
            {
                paged.Size = paging.Size ?? 10;
                paged.Page = paging.Page ?? 1;

                paged.TotalPages = paged.TotalItems / paged.Size;

                if (paged.TotalPages < paged.Page)
                    paged.TotalPages = 1;

                if (paged.TotalItems % paged.Size > 0)
                    paged.TotalPages++;

                paged.Data = await query.Skip((paged.Page - 1) * paged.Size).Take(paged.Size).AsQueryable().ToListAsync();
            }
            return paged;
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> query,
            Func<IQueryable<T>, IOrderedQueryable<T>> order = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            if (include != null)
                query = include(query);

            if (order != null)
                query = order(query);

            return query.AsQueryable();
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> query,
            Func<IQueryable<T>, IQueryable<T>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> order = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            if (filter != null)
                query = filter(query);

            if (include != null)
                query = include(query);

            if (order != null)
                query = order(query);

            return query.AsQueryable();
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> order = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            if (predicate != null)
                query = query.Where(predicate);

            if (include != null)
                query = include(query);

            if (order != null)
                query = order(query);

            return query.AsQueryable();
        }

        public static IQueryable<T> Query<T>(this IQueryable<T> query,
            Func<IQueryable<T>, IQueryable<T>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> order = null,
            Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            if (filter != null)
                query = filter(query);

            if (include != null)
                query = include(query);

            if (order != null)
                query = order(query);

            return query.AsQueryable();
        }

        public static IQueryable<T> Select<T>(this IQueryable<T> source, string[] columns)
        {
            var sourceType = source.ElementType;
            var resultType = typeof(T);
            var parameter = Expression.Parameter(sourceType, "e");
            var bindings = columns.Select(column => Expression.Bind(resultType.GetProperty(column), Expression.PropertyOrField(parameter, column)));
            var body = Expression.MemberInit(Expression.New(resultType), bindings);
            var selector = Expression.Lambda(body, parameter);
            var query = source.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, resultType }, source.Expression, Expression.Quote(selector)));
            return query;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, Dictionary<string, string> sorts)
        {
            var expression = source.Expression;
            int count = 0;
            foreach (var item in sorts)
            {
                var parameter = Expression.Parameter(typeof(T));
                var selector = Expression.PropertyOrField(parameter, item.Key);
                var method = string.Equals(item.Value, "desc", StringComparison.OrdinalIgnoreCase) ? (count == 0 ? "OrderByDescending" : "ThenByDescending") : (count == 0 ? "OrderBy" : "ThenBy");
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }
            return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
        }

        //Use Set<T>
        static readonly MethodInfo SetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set));

        public static IQueryable Query(this DbContext context, string entityName) => context.Query(context.Model.FindEntityType(entityName).ClrType);

        public static IQueryable Query(this DbContext context, Type entityType) => (IQueryable)SetMethod.MakeGenericMethod(entityType).Invoke(context, null);

        //Use reflection
        //public static IQueryable Query(this DbContext context, string entityName) => context.Query(context.Model.FindEntityType(entityName).ClrType);
        //public static IQueryable Query(this DbContext context, Type entityType) => (IQueryable)((IDbSetCache)context).GetOrAddSet(context.GetDependencies().SetSource, entityType);
    }
}
