using System.Text.RegularExpressions;

namespace Jarvis.Caching;

public class CacheEntryOption
{
    /// <summary>
    /// Key of the cache item
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The duration this cache item resides in application's memory.
    /// If null then this cache item will not be set in memory
    /// </summary>
    public int MemSeconds { get; set; } = 0;

    /// <summary>
    /// The time this cache item resides in a distributed cache.
    /// If null then this cache item will not be set in distributed cache
    /// </summary>
    public int DistSeconds { get; set; } = 0;

    /// <summary>
    /// The name of the distributed cache group
    /// </summary>
    public string DistGroup { get; set; } = string.Empty;

    /// <summary>
    /// Type of the distributed cache (Redis, CouchBase, MemCache,...)
    /// </summary>
    public string DistType { get; set; } = string.Empty;

    public bool MemSecondsSet => MemSeconds > 0;

    public bool DistSecondsSet => DistSeconds > 0;

    /// <summary>
    /// Return values of this config item (Key, DistType, DistGroup, MemExpires, DistExpires)
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    internal (string, string, string, TimeSpan?, TimeSpan?) GetConfigValues(CacheParam param)
    {
        string key = Key;
        if (param.HasParams)
        {
            key = Regex.Replace(key, @"\{([^{}]+)\}", match =>
            {
                string key = match.Groups[1].Value;
                if (param.Params.ContainsKey(key))
                    return param.Params[key];
                else
                    return match.Value;
            });
        }
        TimeSpan? memExpires = param.MemExpiresSet ? param.MemoryExpiresIn : MemSecondsSet ? TimeSpan.FromSeconds(MemSeconds) : null;
        TimeSpan? distExpires = param.DistExpiresSet ? param.DistributedExpiresIn : DistSecondsSet ? TimeSpan.FromSeconds(DistSeconds) : null;
        return (key, DistType, DistGroup, memExpires, distExpires);
    }
}