using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

/// <summary>
/// Subscribes to cross-node memory cache invalidation messages and removes entries on peer instances.
/// </summary>
public sealed class RedisMemoryCacheInvalidationSubscriber(
    IConnectionMultiplexer connectionMultiplexer,
    IMemoryCache memoryCache,
    ILogger<RedisMemoryCacheInvalidationSubscriber> logger)
    : BackgroundService, IMemoryCacheInvalidationSubscriber
{
    private static readonly JsonSerializerOptions SerializerOptions = new();

    private readonly IConnectionMultiplexer _muxer = connectionMultiplexer;
    private readonly IMemoryCache _memCache = memoryCache;
    private readonly ILogger<RedisMemoryCacheInvalidationSubscriber> _logger = logger;

    public async Task RemoveAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.Equals(info.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            return;

        await _memCache.RemoveAsync(info.Key, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _muxer.GetSubscriber()
            .SubscribeAsync(RedisChannel.Literal(MemoryCacheInvalidationDefaults.RedisChannel),
                async (_, message) =>
                {
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    try
                    {
                        var msg = message.ToString();
                        if (string.IsNullOrEmpty(msg))
                            return;

                        var info = JsonSerializer.Deserialize<MemoryCacheInvalidationInfo>(msg, SerializerOptions);
                        if (info is null)
                            return;

                        await RemoveAsync(info, stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to invalidate memory cache for message {Message}", message);
                    }
                })
            .ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _muxer.GetSubscriber()
            .UnsubscribeAsync(RedisChannel.Literal(MemoryCacheInvalidationDefaults.RedisChannel))
            .ConfigureAwait(false);
    }
}
