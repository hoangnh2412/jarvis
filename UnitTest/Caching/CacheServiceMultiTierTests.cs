using Jarvis.Caching;

namespace UnitTest.Caching;

public class CacheServiceMultiTierTests
{
    private static JarvisCacheOptions CreateLayeredOptions() => new()
    {
        DefaultDistributedType = "Redis",
        DefaultDistributedGroup = "Default",
        Items =
        {
            ["Layered"] = new CacheEntryOption
            {
                Key = "layer:{id}",
                MemSeconds = 60,
                DistributedSeconds = 300,
                DistributedGroup = "Default",
                DistributedType = "Redis",
            },
        },
    };

    private static (CacheService Service, FakeMemoryCache Memory, FakeDistributedCache Distributed) CreateService()
    {
        var memory = new FakeMemoryCache();
        var distributed = new FakeDistributedCache();
        var registry = new DistributedCacheRegistry();
        registry.Caches["Redis:Default"] = distributed;

        var service = new CacheService(CreateLayeredOptions());
        service.SetMemCache(memory);
        service.SetDistributedCacheRegistry(registry);
        return (service, memory, distributed);
    }

    [Fact]
    public async Task TryGetAsync_MemoryHit_DoesNotQueryDistributed()
    {
        var (service, memory, distributed) = CreateService();
        var param = CacheParam.Create("Layered").WithParam("id", "1");
        var resolution = service.Resolve(param);
        Assert.NotNull(resolution);

        await memory.SetAsync(resolution!.Value.Key, "from-memory", resolution.Value.MemoryExpires);

        var hit = await service.TryGetAsync<string>(param);

        Assert.True(hit.HasValue);
        Assert.Equal("from-memory", hit.Value);
        Assert.Equal(0, distributed.TryGetCount);
    }

    [Fact]
    public async Task TryGetAsync_DistributedHit_BackfillsMemory()
    {
        var (service, memory, distributed) = CreateService();
        var param = CacheParam.Create("Layered").WithParam("id", "2");
        var resolution = service.Resolve(param);
        Assert.NotNull(resolution);

        await distributed.SetAsync(
            resolution!.Value.Key,
            "from-redis",
            resolution.Value.DistributedExpires);

        var hit = await service.TryGetAsync<string>(param);

        Assert.True(hit.HasValue);
        Assert.Equal("from-redis", hit.Value);
        Assert.Equal(1, distributed.TryGetCount);
        Assert.Equal(1, memory.SetCount);

        var memoryHit = await memory.TryGetAsync<string>(resolution.Value.Key);
        Assert.True(memoryHit.HasValue);
        Assert.Equal("from-redis", memoryHit.Value);
    }

    [Fact]
    public async Task GetOrSetAsync_MissBothLayers_InvokesQueryOnce_AndSetsBoth()
    {
        var (service, memory, distributed) = CreateService();
        var param = CacheParam.Create("Layered").WithParam("id", "3");
        var queryCount = 0;

        var value = await service.GetOrSetAsync(param, async _ =>
        {
            queryCount++;
            await Task.Delay(1);
            return "from-db";
        });

        Assert.Equal("from-db", value);
        Assert.Equal(1, queryCount);
        Assert.Equal(1, memory.SetCount);
        Assert.Equal(1, distributed.SetCount);

        var second = await service.GetOrSetAsync(param, async _ =>
        {
            queryCount++;
            return "other";
        });

        Assert.Equal("from-db", second);
        Assert.Equal(1, queryCount);
        Assert.Equal(1, distributed.TryGetCount);
    }
}
