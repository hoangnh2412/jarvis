using Jarvis.Caching.Internal;

namespace Jarvis.Caching;

public class CacheEntryOption
{
    /// <summary>
    /// Key template with optional placeholders, e.g. <c>Product:{tenantId}:{id}</c>.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Seconds in memory. <c>0</c> = do not write to memory (reads still allowed).
    /// </summary>
    public int MemSeconds { get; set; }

    /// <summary>
    /// Seconds in distributed cache. <c>0</c> = do not use Redis/distributed layer.
    /// </summary>
    public int DistributedSeconds { get; set; }

    public string DistributedGroup { get; set; } = string.Empty;

    public string DistributedType { get; set; } = string.Empty;

    public bool MemSecondsSet => MemSeconds > 0;

    public bool DistributedSecondsSet => DistributedSeconds > 0;

    internal CacheEntryConfigValues GetConfigValues(CacheParam param)
    {
        var key = CacheKeyResolver.Resolve(Key, param.Params);
        var memExpires = param.MemExpiresSet ? param.MemoryExpiresIn : MemSecondsSet ? TimeSpan.FromSeconds(MemSeconds) : null;
        var distributedExpires = param.DistributedExpiresSet ? param.DistributedExpiresIn : DistributedSecondsSet ? TimeSpan.FromSeconds(DistributedSeconds) : null;
        var useDistributed = distributedExpires is not null;
        return new CacheEntryConfigValues(key, DistributedType, DistributedGroup, memExpires, distributedExpires, useDistributed);
    }
}
