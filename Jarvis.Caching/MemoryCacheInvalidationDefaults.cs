namespace Jarvis.Caching;

public static class MemoryCacheInvalidationDefaults
{
    public const string ConnectionServiceKey = "Jarvis.Cache.MemoryInvalidation";

    public const string RedisChannel = "memcache-invalidation";
}
