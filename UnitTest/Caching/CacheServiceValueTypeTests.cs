using Jarvis.Caching;

namespace UnitTest.Caching;

public class CacheServiceValueTypeTests
{
    [Fact]
    public async Task GetAsync_CachesValueTypeZero()
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
        var registry = new DistributedCacheRegistry();
        var service = new CacheService(options);
        service.SetMemCache(memory);
        service.SetDistributedCacheRegistry(registry);

        var param = CacheParam.Create("Counter").WithParam("id", "1");

        await service.SetAsync(param, 0);
        var value = await service.GetAsync<int>(param);

        Assert.Equal(0, value);
        Assert.Equal(1, memory.SetCount);
    }
}
