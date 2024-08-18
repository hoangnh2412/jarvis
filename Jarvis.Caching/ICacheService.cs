namespace Jarvis.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(CacheParam param, CancellationToken cancellationToken = default);
    
    Task<T?> GetAsync<T>(CacheParam param, Func<Task<T>> query, CancellationToken cancellationToken = default);
    
    Task SetAsync<T>(CacheParam param, T data, CancellationToken cancellationToken = default);
    
    Task RemoveAsync(CacheParam param, CancellationToken cancellationToken = default);
}
