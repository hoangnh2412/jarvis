namespace Jarvis.Caching;

/// <summary>
/// Redis pub/sub used only for cross-node memory cache invalidation (separate from distributed cache clusters).
/// </summary>
public class CacheMemoryInvalidationOptions
{
    public CacheMemoryInvalidationRedisOptions Redis { get; set; } = new();
}

public class CacheMemoryInvalidationRedisOptions
{
    /// <summary>
    /// StackExchange.Redis configuration string for the dedicated invalidation connection.
    /// </summary>
    public string Configuration { get; set; } = string.Empty;
}
