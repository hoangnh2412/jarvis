using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Realtime.Configuration;
using Platform.Realtime.Contracts;
using Platform.Realtime.Groups;
using Platform.Realtime.Services;
using Platform.Realtime.Sse.Internal;
using StackExchange.Redis;

namespace Platform.Realtime.Sse.Services;

public sealed class SseRealtimeNotifier(
    SseConnectionRegistry registry,
    IOptions<PlatformRealtimeOptions> options,
    IServiceProvider serviceProvider,
    ILogger<SseRealtimeNotifier> logger) : IRealtimeTransport
{
    public Task PublishToUserAsync(
        Guid userId,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return PublishToGroupAsync(RealtimeGroupNames.ForUser(userId), message, cancellationToken);
    }

    public async Task PublishToUsersAsync(
        IEnumerable<Guid> userIds,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentNullException.ThrowIfNull(message);
        foreach (var userId in userIds.Distinct())
            await PublishToUserAsync(userId, message, cancellationToken).ConfigureAwait(false);
    }

    public Task PublishToGroupAsync(
        string groupName,
        object message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);

        var eventName = GetClientMethodName();
        var jsonData = JsonSerializer.Serialize(message, SseConnectionRegistry.JsonOptions);

        if (options.Value.Sse.UseRedisFanOut)
            return PublishViaRedisFanOutAsync(groupName, eventName, jsonData);

        return SseLocalPublisher.DeliverToGroupAsync(registry, groupName, eventName, jsonData, logger, cancellationToken);
    }

    private async Task PublishViaRedisFanOutAsync(string groupName, string eventName, string jsonData)
    {
        var redisMultiplexer = serviceProvider.GetService<IRealtimeRedisMultiplexer>()
            ?? throw new InvalidOperationException(
                "IRealtimeRedisMultiplexer is required when Realtime:Sse:UseRedisFanOut is enabled. Call UseSseRedisFanOut().");

        var channelPrefix = options.Value.Sse.ChannelPrefix;
        if (string.IsNullOrWhiteSpace(channelPrefix))
            channelPrefix = new RealtimeSseOptions().ChannelPrefix;

        var payload = JsonSerializer.Serialize(
            new SseFanOutPayload(groupName, eventName, jsonData),
            SseConnectionRegistry.JsonOptions);

        var channel = SseFanOutChannels.ForGroup(channelPrefix, groupName);
        await redisMultiplexer.Multiplexer
            .GetSubscriber()
            .PublishAsync(RedisChannel.Literal(channel), payload)
            .ConfigureAwait(false);
    }

    private string GetClientMethodName()
    {
        return string.IsNullOrWhiteSpace(options.Value.ClientMethodName)
            ? new PlatformRealtimeOptions().ClientMethodName
            : options.Value.ClientMethodName;
    }
}

internal static class SseLocalPublisher
{
    public static Task DeliverToGroupAsync(
        SseConnectionRegistry registry,
        string groupName,
        string eventName,
        string jsonData,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var connections = registry.GetByGroup(groupName);
        if (connections.Count == 0)
            return Task.CompletedTask;

        return Task.WhenAll(connections.Select(connection =>
            WriteSafelyAsync(connection, eventName, jsonData, logger, cancellationToken)));
    }

    public static Task DeliverFanOutPayloadAsync(
        SseConnectionRegistry registry,
        SseFanOutPayload payload,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        return DeliverToGroupAsync(
            registry,
            payload.GroupName,
            payload.EventName,
            payload.JsonData,
            logger,
            cancellationToken);
    }

    private static async Task WriteSafelyAsync(
        SseConnectionEntry connection,
        string eventName,
        string jsonData,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await connection.WriteSseEventAsync(eventName, jsonData, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected.
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to write SSE event to connection {ConnectionId}.", connection.ConnectionId);
        }
    }
}
