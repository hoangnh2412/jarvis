namespace Jarvis.Domain.Shared.Extensions;

/// <summary>
/// Provides extension functions for ICollection
/// </summary>
public static partial class CollectionExtensions
{
    /// <summary>
    /// Paginate and return items by page
    /// </summary>
    /// <param name="collection">Instance of ICollection</param>
    /// <param name="page">Number of pages to retrieve data from</param>
    /// <param name="size">Number of items per page</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> PaginationList<T>(this ICollection<T> collection, int page, int size)
    {
        page = page - 1;
        return collection.Skip(page * size).Take(size).ToList();
    }

    /// <summary>
    /// Paginate and return total pages
    /// </summary>
    /// <param name="collection">Instance of ICollection</param>
    /// <param name="size">Number of items per page</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int PaginationPage<T>(this ICollection<T> collection, int size)
    {
        return collection.Count % size == 0 ? collection.Count / size : collection.Count / size + 1;
    }
}
