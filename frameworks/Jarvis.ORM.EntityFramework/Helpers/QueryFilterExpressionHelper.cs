using System.Linq.Expressions;

namespace Jarvis.ORM.EntityFramework.Helpers;

public class QueryFilterExpressionHelper
{
    public static Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expression1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expression1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expression2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expression2.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
    }

    private class ReplaceExpressionVisitor(
        Expression oldValue,
        Expression newValue)
        : ExpressionVisitor
    {
        private readonly Expression _oldValue = oldValue;
        private readonly Expression _newValue = newValue;

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override Expression Visit(Expression node)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}

public static class QueryFilterExpressionExtensions
{
    /// <summary>
    /// Combines two expressions with a logical AND.
    /// </summary>
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;
        return QueryFilterExpressionHelper.CombineExpressions(left, right);
    }
}