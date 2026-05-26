using Jarvis.Caching;
using Jarvis.Caching.Internal;

namespace UnitTest.Caching;

public class CacheServiceResolutionTests
{
    private static JarvisCacheOptions CreateOptions() => new()
    {
        DefaultDistributedType = "Redis",
        DefaultDistributedGroup = "Default",
        Items =
        {
            ["ConnectionString"] = new CacheEntryOption
            {
                Key = "conn:{dbid}",
                MemSeconds = 14400,
            },
            ["ContextData"] = new CacheEntryOption
            {
                Key = "context:{sid}",
                MemSeconds = 180,
                DistributedSeconds = 14400,
                DistributedGroup = "Auth",
            },
        },
    };

    [Fact]
    public void Resolve_MemoryOnly_DoesNotRequireDistributedGroup()
    {
        var service = new CacheService(CreateOptions());
        var resolution = service.Resolve(CacheParam.Create("ConnectionString").WithParam("dbid", "42"));

        Assert.NotNull(resolution);
        Assert.Equal("conn:42", resolution!.Value.Key);
        Assert.False(resolution.Value.UseDistributed);
        Assert.Equal(TimeSpan.FromSeconds(14400), resolution.Value.MemoryExpires);
    }

    [Fact]
    public void Resolve_Distributed_AppliesDefaults()
    {
        var service = new CacheService(CreateOptions());
        var resolution = service.Resolve(CacheParam.Create("ContextData").WithParam("sid", "abc"));

        Assert.NotNull(resolution);
        Assert.True(resolution!.Value.UseDistributed);
        Assert.Equal("Redis", resolution.Value.DistributedType);
        Assert.Equal("Auth", resolution.Value.DistributedGroup);
        Assert.Equal("context:abc", resolution.Value.Key);
    }

    [Fact]
    public void Resolve_ThrowsWhenPlaceholderMissing()
    {
        var service = new CacheService(CreateOptions());

        Assert.Throws<InvalidOperationException>(() =>
            service.Resolve(CacheParam.Create("ConnectionString")));
    }
}
