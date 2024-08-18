namespace Jarvis.Caching;

public class CacheOption
{
    public Dictionary<string, CacheEntryOption> Items { get; set; } = new Dictionary<string, CacheEntryOption>();

    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> DistGroups { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

    /// <summary>
    /// The default dist group to be used. Eg: Big
    /// </summary>
    public string DefaultDistributedGroup { get; set; } = string.Empty;

    /// <summary>
    /// The default dist type to be used. Eg: Redis
    /// </summary>
    public string DefaultDistributedType { get; set; } = string.Empty;
}