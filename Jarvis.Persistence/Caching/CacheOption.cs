using Jarvis.Persistence.Caching.Redis;

namespace Jarvis.Persistence.Caching;

public class CacheOption
{
    public string DistributedType { get; set; }
    public Dictionary<string, RedisOption> Redis { get; set; }
    public List<CacheEntryOption> Entries { get; set; }
    public Dictionary<string, PolicyEntryOption> Policies { get; set; }
}