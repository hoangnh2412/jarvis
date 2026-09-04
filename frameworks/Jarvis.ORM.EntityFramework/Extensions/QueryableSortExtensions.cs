using System.Linq.Expressions;
using System.Reflection;
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Querying;

namespace Jarvis.ORM.EntityFramework.Extensions;

public static class QueryableSortExtensions
{
    public static IOrderedQueryable<TEntity> ApplyDynamicSort<TEntity>(
        this IQueryable<TEntity> source,
        IReadOnlyList<SortField> sortFields,
        ISet<string>? allowedFields,
        ISet<string>? deniedFields = null)
        where TEntity : class, IEntity
    {
        if (sortFields == null || sortFields.Count == 0)
        {
            return source.ApplyDefaultSort();
        }

        if (allowedFields == null || allowedFields.Count == 0)
        {
            throw new ArgumentException("Dynamic sorting is blocked because no AllowedFields are configured for this query.");
        }

        var type = typeof(TEntity);
        var currentSource = source.Expression;
        var isFirst = true;

        foreach (var field in sortFields)
        {
            if (deniedFields != null && deniedFields.Contains(field.FieldName))
            {
                throw new ArgumentException($"Field '{field.FieldName}' is blocked by blacklist for sorting.");
            }

            if (!allowedFields.Contains(field.FieldName))
            {
                throw new ArgumentException($"Field '{field.FieldName}' is not allowed for sorting.");
            }

            var propertyInfo = type.GetProperty(field.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Field '{field.FieldName}' does not exist on entity '{type.Name}'.");
            }

            var param = Expression.Parameter(type, "e");
            var propAccess = Expression.Property(param, propertyInfo);
            var lambda = Expression.Lambda(propAccess, param);

            var methodPrefix = isFirst ? "OrderBy" : "ThenBy";
            var methodSuffix = field.Direction == SortDirection.Desc ? "Descending" : string.Empty;
            var methodName = methodPrefix + methodSuffix;

            currentSource = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { type, propertyInfo.PropertyType },
                currentSource,
                Expression.Quote(lambda));

            isFirst = false;
        }

        return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(currentSource);
    }
}
