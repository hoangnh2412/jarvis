namespace Jarvis.Caching;

/// <summary>
/// Handles memory cache invalidation messages from other application instances.
/// </summary>
public interface IMemoryCacheInvalidationSubscriber
{
    Task RemoveAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default);
}
