using System.Linq.Expressions;
using System.Reflection;

namespace Jarvis.Shared.Extensions;

public static partial class LambdaExtension
{
    public static void SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
    {
        var memberSelectorExpression = memberLamda.Body as MemberExpression;
        if (memberSelectorExpression != null)
        {
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                property.SetValue(target, value, null);
            }
        }
    }

    public static KeyValuePair<Expression<Func<T, object>>, object> Set<T>(this T target, Expression<Func<T, object>> key, object value)
    {
        return new KeyValuePair<Expression<Func<T, object>>, object>(key, value);
    }
}