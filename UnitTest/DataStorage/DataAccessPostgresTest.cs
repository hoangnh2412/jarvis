using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;
using Sample.DataStorage.EntityFramework;

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
        services.AddSingleton<IHttpContextAccessor>((sp) =>
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = CreateHttpContext();
            return accessor;
        });
        services.AddCorePersistence(Configuration);
        services.AddEFMultiTenancy();
        services.AddEFTenantDbContext();
        services.AddEFSampleDbContext();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "fb38afa6-593e-44a3-ad28-8fc786ca1be9";
        return httpContext;
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

    [TestMethod]
    public async Task Test_DataStorage_MultiTenancy()
    {
        try
        {
            var uowTenant = _serviceProvider.GetService<ITenantUnitOfWork>();
            var repoTenant = uowTenant.GetRepository<IRepository<Tenant>>();
            var tenants = await repoTenant.GetQuery().ToListAsync();

            using (var scope = _serviceProvider.CreateScope())
            {
                var uowApp = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
                var repoUser = uowApp.GetRepository<IRepository<User>>();

                var items = await repoUser.ListAsync();
                Assert.AreNotEqual(0, items.Count);
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}