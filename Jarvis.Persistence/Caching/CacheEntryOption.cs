namespace Jarvis.Persistence.Caching;

public class CacheEntryOption
{
    public string Key { get; set; }
    public string ExpirePolicy { get; set; }
    public string StorageLocation { get; set; }
    public int DistributedCacheSeconds { get; set; }
    public int MemoryCacheSeconds { get; set; }
}