using Newtonsoft.Json;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public class RedisMemoryCacheInvalidatorPublisher : IMemoryCacheInvalidatorPublisher
{
    private readonly string _configuration;
    public RedisMemoryCacheInvalidatorPublisher(string configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishAsync(MemoryCacheInvalidationInfo info)
    {
        var muxer = await RedisConnectionManager.GetInstance().CreateAsync(_configuration);
        if (muxer == null)
            return;

        await muxer.GetSubscriber().PublishAsync(RedisChannel.Literal(MemoryCacheInvalidationInfo.MemoryCacheInvalidationChannel), JsonConvert.SerializeObject(info));
    }
}