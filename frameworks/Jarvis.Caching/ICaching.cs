namespace Jarvis.Caching;

public interface ICaching
{
    Task<CacheValue<T>> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T data, TimeSpan? expires = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
