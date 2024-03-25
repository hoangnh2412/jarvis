namespace Jarvis.Persistence.Caching;

public class RedisOption
{
    public string InstanceName { get; set; }
    public List<string> EndPoints { get; set; }
    public string Password { get; set; }
    public int ConnectRetry { get; set; }
    public bool AbortOnConnectFail { get; set; }
    public int ConnectTimeout { get; set; }
    public int DefaultDatabase { get; set; }
    public int SyncTimeout { get; set; }
    public string AbsoluteExpiration { get; set; }
    public string AbsoluteExpirationRelativeToNow { get; set; }
    public string SlidingExpiration { get; set; }
}
