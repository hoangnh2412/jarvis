namespace Jarvis.Persistence.Caching;

public class PolicyEntryOption
{
    public int DistributedCacheSeconds { get; set; } = 3600;
    public int MemoryCacheSeconds { get; set; } = 60;
}