using Jarvis.Caching;
using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework.DataStorages;
using UnitTest.Caching;

namespace UnitTest.DataStorages;

public sealed class CachingTenantConnectionStringResolverTests
{
    private static ICacheService CreateCacheService()
    {
        var options = new JarvisCacheOptions
        {
            Items =
            {
                [CachingTenantConnectionStringResolver.DefaultCacheItemName] = new CacheEntryOption
                {
                    Key = "conn:{dbid}",
                    MemSeconds = 3600,
                },
            },
        };

        var memory = new FakeMemoryCache();
        var service = new CacheService(options);
        service.SetMemCache(memory);
        service.SetDistributedCacheRegistry(new DistributedCacheRegistry());
        return service;
    }

    [Fact]
    public async Task GetConnectionStringAsync_CachesSuccessfulLookup_UsingConnDbidKey()
    {
        var inner = new CountingConnectionStringResolver("Server=inner;");
        var resolver = new CachingTenantConnectionStringResolver(inner, CreateCacheService());

        var first = await resolver.GetConnectionStringAsync("tenant-1");
        var second = await resolver.GetConnectionStringAsync("tenant-1");

        Assert.Equal("Server=inner;", first);
        Assert.Equal("Server=inner;", second);
        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public async Task GetConnectionStringAsync_DoesNotCacheNull()
    {
        var inner = new CountingConnectionStringResolver(null);
        var resolver = new CachingTenantConnectionStringResolver(inner, CreateCacheService());

        await resolver.GetConnectionStringAsync("missing");
        await resolver.GetConnectionStringAsync("missing");

        Assert.Equal(2, inner.CallCount);
    }

    private sealed class CountingConnectionStringResolver(string? connectionString) : ITenantConnectionStringResolver
    {
        public int CallCount { get; private set; }

        public Task<string?> GetConnectionStringAsync(string name, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(connectionString);
        }
    }
}
