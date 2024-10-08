using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence;
using Jarvis.Persistence.Caching;
using Microsoft.Extensions.Configuration;
using Jarvis.Persistence.Caching.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;
using Sample.DataStorage.EntityFramework;
using Jarvis.Persistence.Caching.Redis;

namespace UnitTest.Persistences;

[TestClass]
public class PersistenceTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(Configuration);
        services.AddCorePersistence(Configuration);
        services.AddEFSampleDbContext();

        var redisOption = new RedisOption();
        Configuration.GetSection("Redis").Bind(redisOption);
        services.AddRedisCache(redisOption);

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task Test_GetCacheKey()
    {
        var cacheService = _serviceProvider.GetService<ICachingService>();
        var users = await cacheService.GetAsync<List<User>>("users", async () =>
        {
            var uow = _serviceProvider.GetService<ISampleUnitOfWork>();
            var repo = uow.GetRepository<IEFRepository<User>>();

            return await repo.GetQuery().ToListAsync();
        }, TimeSpan.FromMinutes(15));
    }
}