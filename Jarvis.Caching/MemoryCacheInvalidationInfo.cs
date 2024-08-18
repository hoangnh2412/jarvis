namespace Jarvis.Caching;

public class MemoryCacheInvalidationInfo
{
    public static string MemoryCacheInvalidationChannel = "memcache-invalidation";
    
    public string Key { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;
}