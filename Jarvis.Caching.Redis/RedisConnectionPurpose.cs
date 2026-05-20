namespace Jarvis.Caching.Redis;

/// <summary>
/// Separates connection pools so distributed cache and memory invalidation never share a multiplexer instance,
/// even when the configuration string is identical.
/// </summary>
public enum RedisConnectionPurpose
{
    DistributedCache,
    MemoryInvalidation,
}
