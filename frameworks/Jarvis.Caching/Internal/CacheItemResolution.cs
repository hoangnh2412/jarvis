namespace Jarvis.Caching.Internal;

/// <summary>
/// Fully resolved cache item settings used by <see cref="CacheService"/> for get/set/remove.
/// </summary>
internal readonly record struct CacheItemResolution(
    string Key,
    string DistributedType,
    string DistributedGroup,
    TimeSpan? MemoryExpires,
    TimeSpan? DistributedExpires,
    bool UseDistributed);
