using StackExchange.Redis;

namespace Platform.Realtime.Services;

public interface IRealtimeRedisMultiplexer
{
    IConnectionMultiplexer Multiplexer { get; }
}
