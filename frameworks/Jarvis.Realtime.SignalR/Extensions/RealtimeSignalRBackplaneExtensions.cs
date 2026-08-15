using Microsoft.Extensions.DependencyInjection;
using Jarvis.Realtime.Configuration;
using Jarvis.Realtime.Hosting;
using StackExchange.Redis;

namespace Jarvis.Realtime.SignalR.Extensions;

public static class RealtimeSignalRBackplaneExtensions
{
    /// <summary>
    /// Dùng Redis làm backplane scale-out cho SignalR.
    /// </summary>
    public static RealtimeBuilder UseRedisBackplane(
        this RealtimeBuilder builder,
        string? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.SignalRServerBuilder is null)
        {
            throw new InvalidOperationException(
                "Call UseSignalR() before UseRedisBackplane() on RealtimeBuilder.");
        }

        var redisConfig = configuration
            ?? builder.Options.Redis.Configuration
            ?? builder.HostBuilder.Configuration["Realtime:SignalR:Redis:Configuration"];

        if (string.IsNullOrWhiteSpace(redisConfig))
        {
            throw new InvalidOperationException(
                "Realtime:SignalR:Redis:Configuration is required when calling UseRedisBackplane().");
        }

        var channelPrefix = builder.Options.Redis.ChannelPrefix;
        if (string.IsNullOrWhiteSpace(channelPrefix))
        {
            channelPrefix = builder.HostBuilder.Configuration["Realtime:SignalR:Redis:ChannelPrefix"];
        }

        if (string.IsNullOrWhiteSpace(channelPrefix))
            channelPrefix = new RealtimeSignalRRedisOptions().ChannelPrefix;

        builder.SignalRServerBuilder.AddStackExchangeRedis(redisConfig, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal(channelPrefix);
        });

        return builder;
    }
}
