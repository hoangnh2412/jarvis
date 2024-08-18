using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Jarvis.Caching.Redis;

public class RedisMemoryCacheInvalidator(
    string configuration,
    IMemoryCache memoryCache,
    ILogger<RedisMemoryCacheInvalidator> logger)
    : BackgroundService, IMemoryCacheInvalidator
{
    private readonly IMemoryCache _memCache = memoryCache;
    private IConnectionMultiplexer? _muxer;
    private readonly string _configuration = configuration;
    private readonly ILogger<RedisMemoryCacheInvalidator> _logger = logger;

    public async Task RemoveAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default)
    {
        if (info.MachineName != Environment.MachineName)
        {
            await _memCache.RemoveAsync(info.Key, cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _muxer ??= await RedisConnectionManager.GetInstance().CreateAsync(_configuration);
        await _muxer.GetSubscriber().SubscribeAsync(RedisChannel.Literal(MemoryCacheInvalidationInfo.MemoryCacheInvalidationChannel), async (channel, message) =>
        {
            try
            {
                var msg = message.ToString();
                if (string.IsNullOrEmpty(msg))
                    return;

                var info = JsonSerializer.Deserialize<MemoryCacheInvalidationInfo>(msg);
                if (info == null)
                    return;

                await RemoveAsync(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to invalidate mem-cache with key {key}", message);
            }
        });
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_muxer == null)
            return;

        await _muxer.GetSubscriber().UnsubscribeAsync(RedisChannel.Literal(MemoryCacheInvalidationInfo.MemoryCacheInvalidationChannel));
    }
}