using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence;

namespace UnitTest.DataStorage;

[TestClass]
public class DataAccessPostgresTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(Configuration);
        services.AddCorePersistence(Configuration);
        services.AddSampleDbContext();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task Test_DataStorage_GenericRepository_CRUD()
    {
        var uow = _serviceProvider.GetService<ISampleUnitOfWork>();
        var repo = uow.GetRepository<IRepository<User>>();

        // List
        var items = await repo.ListAsync();
        Assert.AreEqual(0, items.Count);

        // Insert
        var random = new Random();
        await repo.InsertAsync(new User
        {
            Name = $"Patient {Guid.NewGuid().ToString("N")}",
            Age = random.Next(20, 50)
        });
        await uow.CommitAsync();
        Assert.AreEqual(items.Count + 1, await repo.CountAsync());
    }
}