namespace Jarvis.Caching;

/// <summary>
/// Holds registered distributed cache implementations keyed by <c>{DistributedType}:{DistributedGroup}</c>.
/// </summary>
public sealed class DistributedCacheRegistry
{
    public Dictionary<string, IDistributedCache> Caches { get; } = new(StringComparer.OrdinalIgnoreCase);
}
