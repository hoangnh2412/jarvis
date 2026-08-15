namespace Jarvis.Caching.Internal;

/// <summary>
/// Resolved cache entry settings from <see cref="CacheEntryOption"/> and <see cref="CacheParam"/> before distributed defaults are applied.
/// </summary>
internal readonly record struct CacheEntryConfigValues(
    string Key,
    string DistributedType,
    string DistributedGroup,
    TimeSpan? MemoryExpires,
    TimeSpan? DistributedExpires,
    bool UseDistributed)
{
    internal CacheItemResolution ToItemResolution(JarvisCacheOptions options, string itemName)
    {
        if (string.IsNullOrEmpty(Key))
            throw new InvalidOperationException("Cache key cannot be empty.");

        var distributedType = DistributedType;
        var distributedGroup = DistributedGroup;

        if (UseDistributed)
        {
            if (string.IsNullOrEmpty(distributedType))
                distributedType = options.DefaultDistributedType;
            if (string.IsNullOrEmpty(distributedGroup))
                distributedGroup = options.DefaultDistributedGroup;

            if (string.IsNullOrEmpty(distributedType) || string.IsNullOrEmpty(distributedGroup))
            {
                throw new InvalidOperationException(
                    $"Cache item '{itemName}' uses distributed cache (DistributedSeconds > 0) but DistributedType/DistributedGroup or Cache:DefaultDistributedType/DefaultDistributedGroup are not configured.");
            }
        }
        else
        {
            distributedType = string.Empty;
            distributedGroup = string.Empty;
        }

        return new CacheItemResolution(Key, distributedType, distributedGroup, MemoryExpires, DistributedExpires, UseDistributed);
    }
}
