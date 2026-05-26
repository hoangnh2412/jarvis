using Jarvis.Caching;
using Microsoft.Extensions.Configuration;

namespace UnitTest.Caching;

public class CacheOptionsBindingTests
{
    [Fact]
    public void Binds_DefaultDistributedGroup_And_DistributedGroups()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cache:DefaultDistributedGroup"] = "Auth",
                ["Cache:DefaultDistributedType"] = "Redis",
                ["Cache:DistributedGroups:Redis:Default:Configuration"] = "127.0.0.1:6379",
            })
            .Build();

        var options = new JarvisCacheOptions();
        configuration.GetSection(JarvisCacheOptions.SectionName).Bind(options);

        Assert.Equal("Auth", options.DefaultDistributedGroup);
        Assert.Equal("Redis", options.DefaultDistributedType);
        Assert.True(options.DistributedGroups.ContainsKey("Redis"));
    }

    [Fact]
    public void Binds_MemoryInvalidation_Redis_Configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cache:MemoryInvalidation:Redis:Configuration"] = "invalidation.redis:6379",
            })
            .Build();

        var options = new JarvisCacheOptions();
        configuration.GetSection(JarvisCacheOptions.SectionName).Bind(options);

        Assert.Equal("invalidation.redis:6379", options.MemoryInvalidation.Redis.Configuration);
    }
}
