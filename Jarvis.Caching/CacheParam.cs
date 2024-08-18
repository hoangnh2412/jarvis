namespace Jarvis.Caching;

public class CacheParam
{
    public string Name { get; internal set; } = string.Empty;
    public TimeSpan? MemoryExpiresIn { get; internal set; }
    public TimeSpan? DistributedExpiresIn { get; internal set; }
    public Dictionary<string, string> Params { get; internal set; } = new Dictionary<string, string>();
    public bool HasParams => Params is not null and { Count: > 0 };
    public bool MemExpiresSet => MemoryExpiresIn is not null && !MemoryExpiresIn.Equals(TimeSpan.Zero);
    public bool DistExpiresSet => DistributedExpiresIn is not null && !DistributedExpiresIn.Equals(TimeSpan.Zero);

    public static CacheParam Create(string name) => new CacheParam().WithName(name);

    public CacheParam WithName(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        Name = name;
        return this;
    }

    public CacheParam MemExpires(TimeSpan expire)
    {
        MemoryExpiresIn = expire;
        return this;
    }

    public CacheParam DistExpires(TimeSpan expire)
    {
        DistributedExpiresIn = expire;
        return this;
    }

    public CacheParam WithParam(string paramName, string paramValue)
    {
        ArgumentNullException.ThrowIfNull(paramName, nameof(paramName));
        ArgumentNullException.ThrowIfNull(paramValue, nameof(paramValue));
        
        Params ??= new Dictionary<string, string>();
        Params[paramName] = paramValue;
        return this;
    }
}