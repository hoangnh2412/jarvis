using StackExchange.Redis;

namespace Platform.Realtime.Sse.Internal;

internal static class SseFanOutChannels
{
    public static string ForGroup(string channelPrefix, string groupName) =>
        $"{channelPrefix}:sse:{groupName}";

    public static RedisChannel Pattern(string channelPrefix) =>
        RedisChannel.Pattern($"{channelPrefix}:sse:*");
}
