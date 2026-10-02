using Microsoft.Extensions.Configuration;
using Platform.Realtime.Configuration;

namespace UnitTest.Realtime;

public class RealtimeOptionsBindingTests
{
    [Fact]
    public void Binds_HubPath_And_Redis_ChannelPrefix()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Realtime:SignalR:HubPath"] = "/hubs/custom",
                ["Realtime:SignalR:Redis:ChannelPrefix"] = "platform-realtime",
            })
            .Build();

        var options = new PlatformRealtimeOptions();
        configuration.GetSection(PlatformRealtimeOptions.SectionName).Bind(options);

        Assert.Equal("/hubs/custom", options.HubPath);
        Assert.Equal("platform-realtime", options.Redis.ChannelPrefix);
    }

    [Fact]
    public void CoreValidator_Succeeds_When_Backplane_Configuration_Missing()
    {
        var validator = new PlatformRealtimeOptionsValidator();
        var result = validator.Validate(null, new PlatformRealtimeOptions
        {
            HubPath = "/hubs/notifications"
        });

        Assert.True(result.Succeeded);
    }
}
