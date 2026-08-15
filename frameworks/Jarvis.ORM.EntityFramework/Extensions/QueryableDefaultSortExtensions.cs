// Pipeline Step 4: Applies default deterministic sorting (UpdatedAt/CreatedAt/Id) if no dynamic sort is provided.
using System.Linq.Expressions;
using System.Reflection;
using Jarvis.DDD.Domain.Entities;

namespace Jarvis.ORM.EntityFramework.Extensions;

public static class QueryableDefaultSortExtensions
{
    public static IOrderedQueryable<TEntity> ApplyDefaultSort<TEntity>(this IQueryable<TEntity> source)
        where TEntity : class, IEntity
    {
        var type = typeof(TEntity);
        var ordered = false;
        IOrderedQueryable<TEntity>? orderedSource = null;

        var updatedAtProp = type.GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (typeof(ILogUpdatedEntity).IsAssignableFrom(type) && updatedAtProp != null)
        {
            orderedSource = (IOrderedQueryable<TEntity>)ApplySortExpression(source, type, updatedAtProp, "OrderByDescending");
            ordered = true;
        }

        var createdAtProp = type.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (typeof(ILogCreatedEntity).IsAssignableFrom(type) && createdAtProp != null)
        {
            if (ordered)
            {
                orderedSource = (IOrderedQueryable<TEntity>)ApplySortExpression(orderedSource!, type, createdAtProp, "ThenBy");
            }
            else
            {
                orderedSource = (IOrderedQueryable<TEntity>)ApplySortExpression(source, type, createdAtProp, "OrderBy");
                ordered = true;
            }
        }

        // Tie-break with Id if it exists
        var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (idProp != null)
        {
            var methodName = ordered ? "ThenBy" : "OrderBy";
            orderedSource = (IOrderedQueryable<TEntity>)ApplySortExpression(ordered ? orderedSource! : source, type, idProp, methodName);
            return orderedSource;
        }

        if (!ordered)
        {
            throw new InvalidOperationException(
                $"Entity '{type.Name}' does not implement {nameof(ILogUpdatedEntity)}, {nameof(ILogCreatedEntity)}, nor has an 'Id' property. " +
                $"A deterministic default sort cannot be applied. Please provide an explicit Sort string in the request.");
        }

        return orderedSource!;
    }

    private static IQueryable<TEntity> ApplySortExpression<TEntity>(
        IQueryable<TEntity> source,
        Type entityType,
        PropertyInfo propInfo,
        string methodName)
    {
        var param = Expression.Parameter(entityType, "e");
        var propAccess = Expression.Property(param, propInfo);
        var lambda = Expression.Lambda(propAccess, param);

        var resultExp = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { entityType, propInfo.PropertyType },
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<TEntity>(resultExp);
    }
}
