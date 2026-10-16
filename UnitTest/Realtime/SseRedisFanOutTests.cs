using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Platform.Authentication;
using Platform.Realtime.Configuration;
using Platform.Realtime.Extensions;
using Platform.Realtime.Groups;
using Platform.Realtime.Services;
using Platform.Realtime.Sse.Extensions;
using Platform.Realtime.Sse.Internal;
using Platform.Realtime.Sse.Services;
using StackExchange.Redis;

namespace UnitTest.Realtime;

public class SseRedisFanOutTests
{
    [Fact]
    public void Validator_Fails_When_FanOut_Enabled_Without_Redis_Configuration()
    {
        var validator = new PlatformRealtimeOptionsValidator();
        var result = validator.Validate(null, new PlatformRealtimeOptions
        {
            SignalR = new RealtimeSignalROptions { SignalRPath = "/hubs/notifications" },
            Sse = new RealtimeSseOptions
            {
                Enabled = true,
                SsePath = "/sse/notifications",
                UseRedisFanOut = true
            },
            Redis = new RealtimeRedisOptions { Configuration = "" }
        });

        Assert.False(result.Succeeded);
        Assert.Contains("Realtime:Redis:Configuration", result.FailureMessage);
    }

    [Fact]
    public async Task UseSseRedisFanOut_Missing_Configuration_Fails_Validation_On_Start()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Realtime:Sse:Enabled"] = "true",
            ["Realtime:Sse:UseRedisFanOut"] = "true",
            ["Realtime:Sse:SsePath"] = "/sse/notifications",
            ["Realtime:SignalR:SignalRPath"] = "/hubs/notifications",
            ["Realtime:Redis:Configuration"] = "",
        });

        builder.AddCoreRealtime()
            .UseSse<CurrentUserInfo, TestTenantInfo>()
            .UseSseRedisFanOut();

        await using var app = builder.Build();
        var ex = await Assert.ThrowsAsync<OptionsValidationException>(() => app.StartAsync());
        Assert.Contains("Realtime:Redis:Configuration", ex.Message);
    }

    [Fact]
    public async Task Publish_With_FanOut_Publishes_To_Redis_Only()
    {
        ConnectionMultiplexer multiplexer;
        try
        {
            multiplexer = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:6379,abortConnect=false,connectTimeout=2000");
            if (!multiplexer.IsConnected)
                return;
        }
        catch (RedisConnectionException)
        {
            return;
        }

        var registry = new SseConnectionRegistry();
        var group = RealtimeGroupNames.ForUser(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        registry.Register(Guid.NewGuid().ToString("D"), [group], context.Response);

        var bodyBefore = context.Response.Body.Length;
        var options = Options.Create(new PlatformRealtimeOptions
        {
            ClientMethodName = "notification",
            Sse = new RealtimeSseOptions
            {
                UseRedisFanOut = true,
                ChannelPrefix = "platform-realtime-sse-test"
            }
        });

        var notifier = new SseRealtimeNotifier(
            registry,
            options,
            new ServiceCollection()
                .AddSingleton<IRealtimeRedisMultiplexer>(new TestRealtimeRedisMultiplexer(multiplexer))
                .BuildServiceProvider(),
            NullLogger<SseRealtimeNotifier>.Instance);

        var channelPrefix = options.Value.Sse.ChannelPrefix;
        var channel = SseFanOutChannels.ForGroup(channelPrefix, group);
        var delivered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var subscriber = multiplexer.GetSubscriber();
        Action<RedisChannel, RedisValue> handler = (redisChannel, message) =>
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<SseFanOutPayload>(
                message.ToString(),
                SseConnectionRegistry.JsonOptions);
            if (payload is null)
                return;

            _ = SseLocalPublisher.DeliverFanOutPayloadAsync(
                registry,
                payload,
                NullLogger<SseRealtimeNotifier>.Instance);
            delivered.TrySetResult(true);
        };
        await subscriber.SubscribeAsync(RedisChannel.Literal(channel), handler);

        try
        {
            await notifier.PublishToGroupAsync(group, new { Title = "Hello" });
            Assert.Equal(bodyBefore, context.Response.Body.Length);

            await delivered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(context.Response.Body.Length > bodyBefore);
        }
        finally
        {
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(channel));
            await multiplexer.CloseAsync();
        }
    }

    private sealed class TestRealtimeRedisMultiplexer(IConnectionMultiplexer multiplexer) : IRealtimeRedisMultiplexer
    {
        public IConnectionMultiplexer Multiplexer { get; } = multiplexer;
    }

    private sealed class TestTenantInfo : Platform.DDD.Domain.Services.ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }
}
