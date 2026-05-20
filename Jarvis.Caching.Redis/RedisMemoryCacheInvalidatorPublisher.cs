using System.Text.Json;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public sealed class RedisMemoryCacheInvalidationPublisher(IConnectionMultiplexer connectionMultiplexer)
    : IMemoryCacheInvalidationPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new();

    private readonly IConnectionMultiplexer _muxer = connectionMultiplexer;

    public async Task PublishAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = JsonSerializer.Serialize(info, SerializerOptions);
        await _muxer.GetSubscriber()
            .PublishAsync(RedisChannel.Literal(MemoryCacheInvalidationDefaults.RedisChannel), payload)
            .ConfigureAwait(false);
    }
}
