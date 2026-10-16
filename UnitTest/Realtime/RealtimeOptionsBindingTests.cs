using Microsoft.Extensions.Configuration;
using Platform.Realtime.Configuration;
using Platform.Realtime.Extensions;

namespace UnitTest.Realtime;

public class RealtimeOptionsBindingTests
{
    [Fact]
    public void Binds_Nested_SignalR_And_Redis_Options()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Realtime:ClientMethodName"] = "alert",
                ["Realtime:Redis:Configuration"] = "127.0.0.1:6379",
                ["Realtime:SignalR:SignalRPath"] = "/hubs/custom",
                ["Realtime:SignalR:ChannelPrefix"] = "platform-realtime",
                ["Realtime:Sse:SsePath"] = "/sse/custom",
            })
            .Build();

        var options = new PlatformRealtimeOptions();
        configuration.GetSection(PlatformRealtimeOptions.SectionName).Bind(options);

        Assert.Equal("alert", options.ClientMethodName);
        Assert.Equal("127.0.0.1:6379", options.Redis.Configuration);
        Assert.Equal("/hubs/custom", options.SignalR.SignalRPath);
        Assert.Equal("platform-realtime", options.SignalR.ChannelPrefix);
        Assert.Equal("/sse/custom", options.Sse.SsePath);
    }

    [Fact]
    public void CoreValidator_Succeeds_When_Backplane_Configuration_Missing()
    {
        var validator = new PlatformRealtimeOptionsValidator();
        var result = validator.Validate(null, new PlatformRealtimeOptions
        {
            SignalR = new RealtimeSignalROptions { SignalRPath = "/hubs/notifications" },
            Sse = new RealtimeSseOptions { SsePath = "/sse/notifications" }
        });

        Assert.True(result.Succeeded);
    }
}
