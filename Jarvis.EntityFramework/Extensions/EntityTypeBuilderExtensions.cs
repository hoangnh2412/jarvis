using System.Linq.Expressions;
using System.Reflection;
using Jarvis.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace Jarvis.EntityFramework.Extensions;

#nullable disable
#pragma warning disable EF1001 // Internal EF Core API usage.
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// This method is used to add a query filter to this entity which combine with EF Core query filters.
    /// </summary>
    /// <returns></returns>
    public static EntityTypeBuilder<TEntity> AddQueryFilter<TEntity>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, bool>> filter)
        where TEntity : class
    {
        var queryFilterAnnotation = builder.Metadata.FindAnnotation(CoreAnnotationNames.QueryFilter);
        if (queryFilterAnnotation != null && queryFilterAnnotation.Value != null && queryFilterAnnotation.Value is Expression<Func<TEntity, bool>> existingFilter)
        {
            filter = QueryFilterExpressionHelper.CombineExpressions(filter, existingFilter);
        }

        return builder.HasQueryFilter(filter);
    }

    /// <summary>
    /// Add query filter to an Entity type without overriding existing ones
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="entityTypeBuilder">EF Core entity type builder</param>
    /// <param name="expression">The expression that represents the filter</param>
    internal static void AddQueryFilter<T>(this EntityTypeBuilder entityTypeBuilder, Expression<Func<T, bool>> expression)
    {
        var parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
        var expressionFilter = ReplacingExpressionVisitor.Replace(
            expression.Parameters.Single(), parameterType, expression.Body);

        var internalEntityTypeBuilder = entityTypeBuilder.GetInternalEntityTypeBuilder();
        if (internalEntityTypeBuilder.Metadata.GetQueryFilter() != null)
        {
            var currentQueryFilter = internalEntityTypeBuilder.Metadata.GetQueryFilter();
            var currentExpressionFilter = ReplacingExpressionVisitor.Replace(currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
            expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
        }

        var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
        entityTypeBuilder.HasQueryFilter(lambdaExpression);
    }

    internal static InternalEntityTypeBuilder GetInternalEntityTypeBuilder(this EntityTypeBuilder entityTypeBuilder)
    {
        var internalEntityTypeBuilder = typeof(EntityTypeBuilder)
            .GetProperty("Builder", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(entityTypeBuilder) as InternalEntityTypeBuilder;

        return internalEntityTypeBuilder;
    }
}
#pragma warning restore EF1001 // Internal EF Core API usage.
