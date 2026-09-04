namespace Jarvis.Caching;

/// <summary>
/// Cache module options bound from the <c>Cache</c> configuration section.
/// </summary>
public class JarvisCacheOptions
{
    public const string SectionName = "Cache";

    public Dictionary<string, CacheEntryOption> Items { get; set; } = [];

    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> DistributedGroups { get; set; } = [];

    /// <summary>
    /// Default distributed cache group (e.g. Default, Auth, Big). Used when an item has <c>DistributedSeconds &gt; 0</c> and no <c>DistributedGroup</c>.
    /// </summary>
    public string DefaultDistributedGroup { get; set; } = string.Empty;

    /// <summary>
    /// Default distributed cache type (e.g. Redis). Used when an item has <c>DistributedSeconds &gt; 0</c> and no <c>DistributedType</c>.
    /// </summary>
    public string DefaultDistributedType { get; set; } = string.Empty;

    /// <summary>
    /// Dedicated Redis connection for memory invalidation pub/sub (<c>Cache:MemoryInvalidation:Redis:Configuration</c>).
    /// Not tied to <see cref="DistributedGroups"/> — choose a cluster explicitly for invalidation traffic.
    /// </summary>
    public CacheMemoryInvalidationOptions MemoryInvalidation { get; set; } = new();
}
