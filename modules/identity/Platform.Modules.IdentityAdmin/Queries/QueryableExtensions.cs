namespace Platform.Modules.IdentityAdmin.Queries;

internal static class QueryableExtensions
{
    /// <summary>
    /// Materialize without referencing EF Core: EF queries implement <see cref="IAsyncEnumerable{T}"/>,
    /// other sources (in-memory) fall back to a synchronous <c>ToList</c>.
    /// </summary>
    public static async Task<List<T>> MaterializeAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken)
    {
        if (query is not IAsyncEnumerable<T> source)
            return query.ToList();

        var items = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
            items.Add(item);
        return items;
    }
}
