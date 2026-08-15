// Pipeline Step 2: Translates AST filter nodes into EF Core expression trees for safe SQL WHERE clauses.
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Querying;

namespace Jarvis.ORM.EntityFramework.Extensions;

public static class QueryableFilterExtensions
{
    // Reflection Cache to avoid repetitive GetProperty calls
    private static readonly ConcurrentDictionary<(Type EntityType, string FieldName), PropertyInfo?> PropertyCache = new();
    
    private static readonly PropertyInfo EfFunctionsProperty = typeof(EF).GetProperty(nameof(EF.Functions))!;
    private static readonly MethodInfo EfLikeMethod = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) })!;

    public static IQueryable<TEntity> ApplyDynamicFilter<TEntity>(
        this IQueryable<TEntity> source,
        FilterNode? filterAst,
        ISet<string>? allowedFields,
        ISet<string>? deniedFields = null)
        where TEntity : class, IEntity
    {
        if (filterAst == null)
            return source;

        if (allowedFields == null || allowedFields.Count == 0)
            throw new ArgumentException("Dynamic filtering is blocked because no AllowedFields are configured for this query.");

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var filterExpression = BuildExpression<TEntity>(filterAst, parameter, allowedFields, deniedFields);

        if (filterExpression == null)
            return source;

        var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
        return source.Where(lambda);
    }

    private static Expression? BuildExpression<TEntity>(FilterNode node, ParameterExpression parameter, ISet<string> allowedFields, ISet<string>? deniedFields)
        where TEntity : class, IEntity
    {
        return node switch
        {
            FilterCondition condition => BuildConditionExpression<TEntity>(condition, parameter, allowedFields, deniedFields),
            FilterGroup group => BuildGroupExpression<TEntity>(group, parameter, allowedFields, deniedFields),
            _ => throw new InvalidFilterException($"Unknown filter node type '{node.GetType().Name}'.")
        };
    }

    private static Expression? BuildGroupExpression<TEntity>(FilterGroup group, ParameterExpression parameter, ISet<string> allowedFields, ISet<string>? deniedFields)
        where TEntity : class, IEntity
    {
        if (group.Nodes.Count == 0)
            return null;

        Expression? combined = null;

        foreach (var node in group.Nodes)
        {
            var nodeExpr = BuildExpression<TEntity>(node, parameter, allowedFields, deniedFields);
            if (nodeExpr == null) continue;

            if (combined == null)
            {
                combined = nodeExpr;
            }
            else
            {
                combined = group.Logic == LogicOperator.And
                    ? Expression.AndAlso(combined, nodeExpr)
                    : Expression.OrElse(combined, nodeExpr);
            }
        }

        return combined;
    }

    private static Expression BuildConditionExpression<TEntity>(FilterCondition condition, ParameterExpression parameter, ISet<string> allowedFields, ISet<string>? deniedFields)
        where TEntity : class, IEntity
    {
        if (deniedFields != null && deniedFields.Contains(condition.FieldName))
            throw new InvalidFilterException($"Field '{condition.FieldName}' is blocked by blacklist for filtering.");

        if (!allowedFields.Contains(condition.FieldName))
            throw new InvalidFilterException($"Field '{condition.FieldName}' is not allowed for filtering.");

        var propertyInfo = GetCachedPropertyInfo(typeof(TEntity), condition.FieldName);
        if (propertyInfo == null)
            throw new InvalidFilterException($"Field '{condition.FieldName}' does not exist on entity '{typeof(TEntity).Name}'.");

        var propertyAccess = Expression.Property(parameter, propertyInfo);
        
        return condition.Operator switch
        {
            FilterOperator.Equal => BuildEqual(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.NotEqual => BuildNotEqual(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.GreaterThan => BuildGreaterThan(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.LessThan => BuildLessThan(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.GreaterThanOrEqual => BuildGreaterThanOrEqual(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.LessThanOrEqual => BuildLessThanOrEqual(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.Contains => BuildContains(propertyAccess, condition.Value),
            FilterOperator.NotContains => BuildNotContains(propertyAccess, condition.Value),
            FilterOperator.StartsWith => BuildStartsWith(propertyAccess, condition.Value),
            FilterOperator.EndsWith => BuildEndsWith(propertyAccess, condition.Value),
            FilterOperator.Between => BuildBetween(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.In => BuildIn(propertyAccess, condition.Value, propertyInfo.PropertyType),
            FilterOperator.IsNull => BuildIsNull(propertyAccess),
            FilterOperator.IsNotNull => BuildIsNotNull(propertyAccess),
            _ => throw new InvalidFilterException($"Unsupported operator '{condition.Operator}'.")
        };
    }

    private static PropertyInfo? GetCachedPropertyInfo(Type entityType, string fieldName)
    {
        var normalizedFieldName = fieldName.ToLowerInvariant();
        return PropertyCache.GetOrAdd((entityType, normalizedFieldName), key =>
        {
            return key.EntityType.GetProperty(key.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        });
    }

    private static ConstantExpression GetConstantExpression(object? value, Type targetType)
    {
        var convertedValue = ConvertValue(value, targetType);
        return Expression.Constant(convertedValue, targetType);
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                throw new InvalidFilterException($"Cannot convert null to non-nullable type '{targetType.Name}'.");
            return null;
        }

        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (nonNullableType.IsEnum)
        {
            var strValue = value.ToString();
            if (string.IsNullOrWhiteSpace(strValue)) return null;
            try
            {
                return Enum.Parse(nonNullableType, strValue, ignoreCase: true);
            }
            catch (ArgumentException)
            {
                throw new InvalidFilterException($"Value '{strValue}' is not valid for enum '{nonNullableType.Name}'.");
            }
        }

        if (nonNullableType == typeof(Guid))
        {
            var strValue = value.ToString();
            if (string.IsNullOrWhiteSpace(strValue)) return null;
            if (Guid.TryParse(strValue, out var guid))
                return guid;
            throw new InvalidFilterException($"Value '{strValue}' is not a valid Guid.");
        }

        if (nonNullableType == typeof(DateTime) || nonNullableType == typeof(DateTimeOffset) || nonNullableType == typeof(DateOnly))
        {
             var strValue = value.ToString();
             if (string.IsNullOrWhiteSpace(strValue)) return null;

             var formats = new[] 
             { 
                 "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ", 
                 "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
                 "yyyy-MM-dd" 
             };

             if (nonNullableType == typeof(DateOnly))
             {
                 if (DateOnly.TryParseExact(strValue, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dateOnly))
                     return dateOnly;
                 throw new InvalidFilterException($"Value '{strValue}' is not a valid ISO-8601 DateOnly (expected yyyy-MM-dd).");
             }

             if (nonNullableType == typeof(DateTimeOffset))
             {
                 if (DateTimeOffset.TryParseExact(strValue, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var dto))
                     return dto;
                 throw new InvalidFilterException($"Value '{strValue}' is not a valid ISO-8601 DateTimeOffset.");
             }

             if (DateTime.TryParseExact(strValue, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt))
                 return dt;
             throw new InvalidFilterException($"Value '{strValue}' is not a valid ISO-8601 DateTime.");
        }

        try
        {
            return Convert.ChangeType(value, nonNullableType, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidFilterException($"Cannot convert value '{value}' to type '{nonNullableType.Name}'.", ex);
        }
    }

    private static Expression BuildEqual(Expression left, object? value, Type targetType)
    {
        if (value == null) return BuildIsNull(left);
        return Expression.Equal(left, GetConstantExpression(value, targetType));
    }

    private static Expression BuildNotEqual(Expression left, object? value, Type targetType)
    {
        if (value == null) return BuildIsNotNull(left);
        return Expression.NotEqual(left, GetConstantExpression(value, targetType));
    }

    private static Expression BuildGreaterThan(Expression left, object? value, Type targetType)
    {
        return Expression.GreaterThan(left, GetConstantExpression(value, targetType));
    }

    private static Expression BuildLessThan(Expression left, object? value, Type targetType)
    {
        return Expression.LessThan(left, GetConstantExpression(value, targetType));
    }

    private static Expression BuildGreaterThanOrEqual(Expression left, object? value, Type targetType)
    {
        return Expression.GreaterThanOrEqual(left, GetConstantExpression(value, targetType));
    }

    private static Expression BuildLessThanOrEqual(Expression left, object? value, Type targetType)
    {
        return Expression.LessThanOrEqual(left, GetConstantExpression(value, targetType));
    }

    private static string EscapeLikePattern(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Replace("~", "~~")
                    .Replace("%", "~%")
                    .Replace("[", "~[")
                    .Replace("_", "~_");
    }

    private static Expression BuildContains(Expression left, object? value)
    {
        if (left.Type != typeof(string))
            throw new InvalidFilterException("Operator 'contains' can only be used on string fields.");
        
        var strValue = value?.ToString() ?? string.Empty;
        var escapedValue = EscapeLikePattern(strValue);
        var pattern = $"%{escapedValue}%";
        
        return Expression.Call(
            null, 
            EfLikeMethod, 
            Expression.Property(null, EfFunctionsProperty), 
            left, 
            Expression.Constant(pattern), 
            Expression.Constant("~"));
    }

    private static Expression BuildNotContains(Expression left, object? value)
    {
        return Expression.Not(BuildContains(left, value));
    }

    private static Expression BuildStartsWith(Expression left, object? value)
    {
        if (left.Type != typeof(string))
            throw new InvalidFilterException("Operator 'startswith' can only be used on string fields.");
        
        var strValue = value?.ToString() ?? string.Empty;
        var escapedValue = EscapeLikePattern(strValue);
        var pattern = $"{escapedValue}%";
        
        return Expression.Call(
            null, 
            EfLikeMethod, 
            Expression.Property(null, EfFunctionsProperty), 
            left, 
            Expression.Constant(pattern), 
            Expression.Constant("~"));
    }

    private static Expression BuildEndsWith(Expression left, object? value)
    {
        if (left.Type != typeof(string))
            throw new InvalidFilterException("Operator 'endswith' can only be used on string fields.");
        
        var strValue = value?.ToString() ?? string.Empty;
        var escapedValue = EscapeLikePattern(strValue);
        var pattern = $"%{escapedValue}";
        
        return Expression.Call(
            null, 
            EfLikeMethod, 
            Expression.Property(null, EfFunctionsProperty), 
            left, 
            Expression.Constant(pattern), 
            Expression.Constant("~"));
    }

    private static Expression BuildBetween(Expression left, object? value, Type targetType)
    {
        if (value is not object[] array || array.Length != 2)
            throw new InvalidFilterException("Operator 'between' requires an array of exactly 2 values.");

        var val1 = GetConstantExpression(array[0], targetType);
        var val2 = GetConstantExpression(array[1], targetType);

        var greaterEq = Expression.GreaterThanOrEqual(left, val1);
        var lessEq = Expression.LessThanOrEqual(left, val2);

        return Expression.AndAlso(greaterEq, lessEq);
    }

    private static Expression BuildIn(Expression left, object? value, Type targetType)
    {
        if (value is not object[] array)
            throw new InvalidFilterException("Operator 'in' requires an array of values.");

        if (array.Length == 0)
        {
            // IN empty array is always false
            return Expression.Constant(false);
        }

        // Create a list of the target type
        var listType = typeof(List<>).MakeGenericType(targetType);
        var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

        foreach (var item in array)
        {
            var converted = ConvertValue(item, targetType);
            list.Add(converted);
        }

        var constantList = Expression.Constant(list, listType);
        var containsMethod = listType.GetMethod("Contains")!;

        return Expression.Call(constantList, containsMethod, left);
    }

    private static Expression BuildIsNull(Expression left)
    {
        if (left.Type.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
            throw new InvalidFilterException($"Operator 'isnull' cannot be used on non-nullable value type '{left.Type.Name}'.");
            
        return Expression.Equal(left, Expression.Constant(null, left.Type));
    }

    private static Expression BuildIsNotNull(Expression left)
    {
        if (left.Type.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
            throw new InvalidFilterException($"Operator 'isnotnull' cannot be used on non-nullable value type '{left.Type.Name}'.");
            
        return Expression.NotEqual(left, Expression.Constant(null, left.Type));
    }
}
