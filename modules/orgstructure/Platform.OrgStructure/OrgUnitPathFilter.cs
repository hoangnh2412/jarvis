using System.Linq.Expressions;

namespace Platform.OrgStructure;

/// <summary>
/// Nút đã chọn và nhánh con. Path dạng <c>/ma-goc/ma-con</c>; so khớp thêm <c>/</c> để không dính <c>/a</c> với <c>/ab</c>.
/// </summary>
public static class OrgUnitPathFilter
{
    public static bool Contains(string? path, string selectedPath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(selectedPath))
            return false;

        return string.Equals(path, selectedPath, StringComparison.Ordinal)
            || path.StartsWith(selectedPath + "/", StringComparison.Ordinal);
    }

    public static Expression<Func<T, bool>> InSubtree<T>(Expression<Func<T, string?>> path, string selectedPath)
    {
        ArgumentNullException.ThrowIfNull(path);
        var prefix = selectedPath + "/";
        var body = path.Body;
        var notNull = Expression.NotEqual(body, Expression.Constant(null, typeof(string)));
        var equals = Expression.Equal(body, Expression.Constant(selectedPath));
        var startsWith = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])
            ?? throw new InvalidOperationException("string.StartsWith(string) was not found.");
        var starts = Expression.Call(body, startsWith, Expression.Constant(prefix));
        var match = Expression.AndAlso(notNull, Expression.OrElse(equals, starts));
        return Expression.Lambda<Func<T, bool>>(match, path.Parameters);
    }
}
