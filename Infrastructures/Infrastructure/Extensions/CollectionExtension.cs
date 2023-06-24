using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     AddRange ICollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        public static void AddRange<TInput>(this ICollection<TInput> collection, IEnumerable<TInput> addCollection)
        {
            if (collection == null || addCollection == null)
                return;

            foreach (var item in addCollection)
            {
                collection.Add(item);
            }
        }

        public static List<T> PaginationList<T>(this List<T> collection, int page, int size)
        {
            page = page - 1;
            return collection.Skip(page * size).Take(size).ToList();
        }

        public static int PaginationPage<T>(this List<T> collection, int page, int size)
        {
            return collection.Count % size == 0 ? collection.Count / size : collection.Count / size + 1;
        }
    }
}
