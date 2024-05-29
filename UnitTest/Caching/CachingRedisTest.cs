using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.Persistence.Caching;
using Jarvis.Persistence.Caching.Interfaces;
using Newtonsoft.Json;
using Jarvis.Persistence.Caching.Redis;

namespace UnitTest.Caching;

[TestClass]
public class CachingRedisTest : BaseTest
{
    private IServiceProvider _serviceProvider;
    private readonly string KeyName = "TestKey";
    private readonly string KeyValue = "Hello world";
    private readonly string HashKeyName = "TestHashKey";
    private readonly Dictionary<string, string> HashKeyValue = new Dictionary<string, string> {
        { "field1", "value1" },
        { "field2", "value2" },
        { "field3", "value3" },
    };

    [TestInitialize]
    public async Task TestInitializeAsync()
    {
        var redisOption = new RedisOption();
        Configuration.GetSection("Redis").Bind(redisOption);

        var services = new ServiceCollection();
        services.AddRedisCache(redisOption);
        _serviceProvider = services.BuildServiceProvider();

        var cacheService = _serviceProvider.GetService<ICachingService>();

        // Clear data
        await cacheService.RemoveAsync(KeyName);
        await cacheService.RemoveAsync(HashKeyName);

        // Init data
        await cacheService.SetAsync(KeyName, Encoding.UTF8.GetBytes(KeyValue));
        await cacheService.HashSetAsync(HashKeyName, HashKeyValue);
    }

    [TestMethod]
    public async Task Test_Redis_SetGet_Key()
    {
        var cacheService = _serviceProvider.GetService<ICachingService>();
        var data = await cacheService.GetAsync(KeyName);
        Assert.AreEqual(KeyValue, Encoding.UTF8.GetString(data));
    }

    [TestMethod]
    public async Task Test_Redis_Delete_Key()
    {
        var cacheService = _serviceProvider.GetService<ICachingService>();
        await cacheService.RemoveAsync(KeyName);
        var data = await cacheService.GetAsync(KeyName);
        Assert.IsNull(data);
    }

    [TestMethod]
    public async Task Test_Redis_SetGet_HashKey()
    {
        var cacheService = _serviceProvider.GetService<ICachingService>();

        var data = await cacheService.HashGetAsync(HashKeyName);
        Assert.AreEqual(JsonConvert.SerializeObject(HashKeyValue), JsonConvert.SerializeObject(data));
    }

    [TestMethod]
    public async Task Test_Redis_Delete_HashKey()
    {
        var cacheService = _serviceProvider.GetService<ICachingService>();
        await cacheService.HashDeleteAsync(HashKeyName, "field1");

        var data = await cacheService.HashGetAsync(HashKeyName);
        Assert.AreEqual(JsonConvert.SerializeObject(new Dictionary<string, string> {
            { "field2", "value2" },
            { "field3", "value3" },
        }), JsonConvert.SerializeObject(data));
    }
}