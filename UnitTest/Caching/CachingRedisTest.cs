// using System.Text;
// using Microsoft.Extensions.Caching.Distributed;
// using Microsoft.Extensions.DependencyInjection;
// using Jarvis.Infrastructure.Caching;
// using Jarvis.Infrastructure.Caching.Redis;
// using Microsoft.Extensions.Configuration;

// namespace UnitTest.Caching;

// [TestClass]
// public class CachingRedisTest : BaseTest
// {
//     private IServiceProvider _serviceProvider;

//     [TestInitialize]
//     public void TestInitialize()
//     {
//         var redisOption = new RedisOption();
//         Configuration.GetSection("Redis").Bind(redisOption);

//         var services = new ServiceCollection();
//         services.AddRedisCache(redisOption);
//         _serviceProvider = services.BuildServiceProvider();
//     }

//     [TestMethod]
//     public async Task Test_Redis_SetGet_Key()
//     {
//         var cacheService = _serviceProvider.GetService<ICachingService>();
//         await cacheService.SetAsync(":TestKey", Encoding.UTF8.GetBytes("Hello world"));

//         var data = await cacheService.GetAsync(":TestKey");
//         Assert.AreEqual("Hello world", Encoding.UTF8.GetString(data));
//     }
// }