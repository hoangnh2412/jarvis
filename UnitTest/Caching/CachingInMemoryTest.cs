// using System.Text;
// using Microsoft.Extensions.Caching.Distributed;
// using Microsoft.Extensions.DependencyInjection;
// using Jarvis.Infrastructure.Caching;
// using Jarvis.Infrastructure.Caching.InMemory;

// namespace UnitTest.Caching;

// [TestClass]
// public class CachingInMemoryTest : BaseTest
// {
//     private IServiceProvider _serviceProvider;

//     [TestInitialize]
//     public void TestInitialize()
//     {
//         var services = new ServiceCollection();
//         services.AddInMemoryCache();
//         _serviceProvider = services.BuildServiceProvider();
//     }

//     [TestMethod]
//     public async Task Test_InMemory_SetGet_Key()
//     {
//         var cacheService = _serviceProvider.GetService<ICachingService>();
//         await cacheService.SetAsync("TestKey", Encoding.UTF8.GetBytes("Hello world"));

//         var data = await cacheService.GetAsync("TestKey");
//         Assert.AreEqual("Hello world", Encoding.UTF8.GetString(data));
//     }
// }