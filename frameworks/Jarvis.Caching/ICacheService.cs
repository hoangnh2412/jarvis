namespace Jarvis.Caching;

public interface ICacheService
{
    /// <summary>
    /// Reads memory then distributed cache. Does not invoke a data loader.
    /// On miss returns <c>default(T)</c>, which is indistinguishable from a cached default value type
    /// (e.g. <c>0</c>, <c>false</c>). Prefer <see cref="TryGetAsync{T}"/> when hit/miss matters.
    /// </summary>
    Task<T?> GetAsync<T>(CacheParam param, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a cache hit/miss without loading from the data source.
    /// </summary>
    Task<CacheValue<T>> TryGetAsync<T>(CacheParam param, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache-aside: memory → distributed → <paramref name="query"/> on miss → set when loaded.
    /// The <paramref name="query"/> delegate may call any async source (database, blob storage, HTTP, etc.);
    /// pass <paramref name="cancellationToken"/> through to downstream I/O.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(
        CacheParam param,
        Func<CancellationToken, Task<T>> query,
        CancellationToken cancellationToken = default);

    Task SetAsync<T>(CacheParam param, T data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the entry from memory (and notifies peers), and from distributed cache when configured.
    /// </summary>
    Task RemoveAsync(CacheParam param, CancellationToken cancellationToken = default);
}
