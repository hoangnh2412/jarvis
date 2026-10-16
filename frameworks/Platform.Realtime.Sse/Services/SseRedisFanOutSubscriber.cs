using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Platform.Realtime.Configuration;
using Platform.Realtime.Services;
using Platform.Realtime.Sse.Internal;
using StackExchange.Redis;

namespace Platform.Realtime.Sse.Services;

internal sealed class SseRedisFanOutSubscriber(
    SseConnectionRegistry registry,
    IServiceProvider serviceProvider,
    IOptions<PlatformRealtimeOptions> options,
    ILogger<SseRedisFanOutSubscriber> logger) : IHostedService
{
    private ISubscriber? _subscriber;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Sse.UseRedisFanOut)
            return Task.CompletedTask;

        var channelPrefix = options.Value.Sse.ChannelPrefix;
        if (string.IsNullOrWhiteSpace(channelPrefix))
            channelPrefix = new RealtimeSseOptions().ChannelPrefix;

        var redisMultiplexer = serviceProvider.GetRequiredService<IRealtimeRedisMultiplexer>();
        _subscriber = redisMultiplexer.Multiplexer.GetSubscriber();
        _subscriber.Subscribe(SseFanOutChannels.Pattern(channelPrefix), HandleRedisMessage);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_subscriber is null)
            return Task.CompletedTask;

        var channelPrefix = options.Value.Sse.ChannelPrefix;
        if (string.IsNullOrWhiteSpace(channelPrefix))
            channelPrefix = new RealtimeSseOptions().ChannelPrefix;

        _subscriber.Unsubscribe(SseFanOutChannels.Pattern(channelPrefix));
        return Task.CompletedTask;
    }

    private void HandleRedisMessage(RedisChannel channel, RedisValue message)
    {
        if (message.IsNullOrEmpty)
            return;

        SseFanOutPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SseFanOutPayload>(message.ToString(), SseConnectionRegistry.JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Ignored invalid SSE fan-out payload on channel {Channel}.", channel.ToString());
            return;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.GroupName))
            return;

        _ = SseLocalPublisher.DeliverFanOutPayloadAsync(registry, payload, logger);
    }
}
