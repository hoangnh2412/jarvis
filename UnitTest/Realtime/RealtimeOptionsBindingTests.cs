using Microsoft.Extensions.Configuration;
using Jarvis.Realtime.Configuration;

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
                ["Realtime:SignalR:Redis:ChannelPrefix"] = "jarvis-realtime",
            })
            .Build();

        var options = new JarvisRealtimeOptions();
        configuration.GetSection(JarvisRealtimeOptions.SectionName).Bind(options);

        Assert.Equal("/hubs/custom", options.HubPath);
        Assert.Equal("jarvis-realtime", options.Redis.ChannelPrefix);
    }

    [Fact]
    public void CoreValidator_Succeeds_When_Backplane_Configuration_Missing()
    {
        var validator = new JarvisRealtimeOptionsValidator();
        var result = validator.Validate(null, new JarvisRealtimeOptions
        {
            HubPath = "/hubs/notifications"
        });

        Assert.True(result.Succeeded);
    }
}
