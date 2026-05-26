using Jarvis.Caching;

namespace UnitTest.Caching;

public class CacheServiceGetOrSetTests
{
    [Fact]
    public async Task GetOrSetAsync_InvokesQueryOnceOnMiss_ThenHitsCache()
    {
        var options = new JarvisCacheOptions
        {
            Items =
            {
                ["Counter"] = new CacheEntryOption
                {
                    Key = "counter_{id}",
                    MemSeconds = 60,
                },
            },
        };

        var memory = new FakeMemoryCache();
        var service = new CacheService(options);
        service.SetMemCache(memory);
        service.SetDistributedCacheRegistry(new DistributedCacheRegistry());

        var param = CacheParam.Create("Counter").WithParam("id", "1");
        var queryCount = 0;

        var first = await service.GetOrSetAsync(param, async _ =>
        {
            queryCount++;
            await Task.Delay(1);
            return 42;
        });

        var second = await service.GetOrSetAsync(param, async _ =>
        {
            queryCount++;
            return 99;
        });

        Assert.Equal(42, first);
        Assert.Equal(42, second);
        Assert.Equal(1, queryCount);
    }

    [Fact]
    public async Task TryGetAsync_ReturnsMiss_WhenNotCached()
    {
        var options = new JarvisCacheOptions
        {
            Items =
            {
                ["Counter"] = new CacheEntryOption { Key = "counter_{id}", MemSeconds = 60 },
            },
        };

        var service = new CacheService(options);
        service.SetMemCache(new FakeMemoryCache());
        service.SetDistributedCacheRegistry(new DistributedCacheRegistry());

        var result = await service.TryGetAsync<int>(
            CacheParam.Create("Counter").WithParam("id", "1"));

        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task TryGetAsync_ReturnsHit_AfterGetOrSet()
    {
        var options = new JarvisCacheOptions
        {
            Items =
            {
                ["Counter"] = new CacheEntryOption { Key = "counter_{id}", MemSeconds = 60 },
            },
        };

        var service = new CacheService(options);
        service.SetMemCache(new FakeMemoryCache());
        service.SetDistributedCacheRegistry(new DistributedCacheRegistry());

        var param = CacheParam.Create("Counter").WithParam("id", "1");
        await service.GetOrSetAsync(param, _ => Task.FromResult(7));

        var result = await service.TryGetAsync<int>(param);

        Assert.True(result.HasValue);
        Assert.Equal(7, result.Value);
    }
}
