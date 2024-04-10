using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Jarvis.Persistence.MultiTenancy;
using Moq;
using Jarvis.Persistence.DataContexts;

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
        services.AddTenantDbContext();
        services.AddSampleDbContext();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = Guid.NewGuid().ToString();
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
    public async Task Test_DataStorage_MultiTenantcy()
    {
        try
        {
            var uowTenant = _serviceProvider.GetService<ITenantUnitOfWork>();
            var repoTenant = uowTenant.GetRepository<IRepository<Tenant>>();

            var uowApp = _serviceProvider.GetService<ISampleUnitOfWork>();
            var repoUser = uowApp.GetRepository<IRepository<User>>();

            var items = await repoUser.ListAsync();
            Assert.AreEqual(0, items.Count);
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
    }
}