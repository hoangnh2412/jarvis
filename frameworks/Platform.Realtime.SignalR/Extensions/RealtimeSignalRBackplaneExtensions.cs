using Microsoft.Extensions.DependencyInjection;
using Platform.Realtime.Configuration;
using Platform.Realtime.Extensions;
using Platform.Realtime.Services;
using Platform.Realtime.Hosting;
using Platform.Realtime.SignalR.Hosting;
using StackExchange.Redis;

namespace Platform.Realtime.SignalR.Extensions;

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

        var holder = builder.HostBuilder.Services
            .Select(d => d.ImplementationInstance)
            .OfType<RealtimeSignalRBuilderHolder>()
            .FirstOrDefault();

        if (holder?.ServerBuilder is null)
        {
            throw new InvalidOperationException(
                "Call UseSignalR() before UseRedisBackplane() on RealtimeBuilder.");
        }

        if (!string.IsNullOrWhiteSpace(configuration))
            builder.Options.Redis.Configuration = configuration;

        builder.EnsureRealtimeRedis();

        var channelPrefix = string.IsNullOrWhiteSpace(builder.Options.SignalR.ChannelPrefix)
            ? new RealtimeSignalROptions().ChannelPrefix
            : builder.Options.SignalR.ChannelPrefix;

        IRealtimeRedisMultiplexer? shared = null;
        builder.HostBuilder.Services.AddSingleton<IRealtimeRedisMultiplexer>(sp =>
            shared ??= sp.GetRequiredService<IRealtimeRedisMultiplexer>()); 
            

        holder.ServerBuilder.AddStackExchangeRedis(options =>
        {
            options.ConnectionFactory = _ =>
                Task.FromResult(shared!.Multiplexer!);
            options.Configuration.ChannelPrefix = RedisChannel.Literal(channelPrefix);
        });

        return builder;
    }
}
