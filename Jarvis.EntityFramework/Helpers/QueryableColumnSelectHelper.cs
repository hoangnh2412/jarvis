using System.Linq.Expressions;
using System.Reflection;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.Helpers;

internal static class QueryableColumnSelectHelper
{
    /// <summary>
    /// Projects each row to a new entity instance with only the listed properties (plus <c>Id</c> when present and not already listed).
    /// Requires a public parameterless constructor on <typeparamref name="TEntity"/>.
    /// </summary>
    public static IQueryable<TEntity> ApplyColumnSelection<TEntity>(
        this IQueryable<TEntity> source,
        string? columns)
        where TEntity : class, IEntity
    {
        var names = PagedListRequestParser.ParseColumns(columns);
        if (names.Count == 0)
            return source;

        var type = typeof(TEntity);
        var nameSet = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (idProp != null && idProp.CanWrite)
            nameSet.Add(idProp.Name);

        var ctor = type.GetConstructor(Type.EmptyTypes)
            ?? throw new InvalidOperationException(
                $"{type.Name} must expose a parameterless constructor when {nameof(PagedListRequest.Columns)} is set.");

        var param = Expression.Parameter(type, "e");
        var bindings = new List<MemberBinding>();
        foreach (var name in nameSet.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanWrite)
                throw new ArgumentException($"Unknown or non-writable column '{name}' on {type.Name}.", nameof(columns));

            bindings.Add(Expression.Bind(prop, Expression.Property(param, prop)));
        }

        var body = Expression.MemberInit(Expression.New(ctor), bindings);
        var lambda = Expression.Lambda<Func<TEntity, TEntity>>(body, param);
        return source.Select(lambda);
    }
}
