namespace Jarvis.Caching.Redis;

public interface IRedisConnectionManagerFactory
{
    IRedisConnectionManager Create();
}

public class RedisConnectionManagerFactory : IRedisConnectionManagerFactory
{
    public IRedisConnectionManager Create()
    {
        return RedisConnectionManager.GetInstance();
    }
}