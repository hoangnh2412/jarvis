using Jarvis.Domain.DataStorages;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Sample.Entities;
using Sample.Multitenancy;
using Sample.Persistence;
using UnitTest.Infrastructure;

namespace UnitTest.DataStorages;

public sealed class DbTenantConnectionStringResolverTests
{
    [Fact]
    public async Task GetConnectionStringAsync_ReturnsTenantConnectionString_FromMaster()
    {
        var tenantId = MultitenancyEfJobRunner.MasterTenantRegistryId;
        var expected = MultitenancyEfTestDatabaseNames.Tenant(tenantId);
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await MultitenancyEfTestDataSeeder.EnsureTenantRegisteredAsync(provider, tenantId);

            await using var scope = provider.CreateAsyncScope();
            var resolver = scope.ServiceProvider
                .GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(TenantDbContext).Name);

            var connectionString = await resolver.GetConnectionStringAsync(tenantId.ToString());

            Assert.Equal(expected, connectionString);
        }
        finally
        {
            if (provider is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (provider is IDisposable disposable)
                disposable.Dispose();
        }
    }

    [Fact]
    public async Task GetConnectionStringAsync_ReturnsNull_ForInvalidTenantId()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var resolver = scope.ServiceProvider
                .GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(TenantDbContext).Name);

            var connectionString = await resolver.GetConnectionStringAsync("not-a-guid");

            Assert.Null(connectionString);
        }
        finally
        {
            if (provider is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (provider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
