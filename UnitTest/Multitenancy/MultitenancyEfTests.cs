using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Sample.Entities;
using Sample.Multitenancy;
using Sample.Persistence;
using UnitTest.Infrastructure;

namespace UnitTest.Multitenancy;

/// <summary>
/// Mirrors minimum scenarios from <c>Sample/Controllers/MultitenancyEfTestController</c> using InMemory databases.
/// </summary>
public sealed class MultitenancyEfTests
{
    private static readonly Guid MasterTenantId = MultitenancyEfJobRunner.MasterTenantRegistryId;
    private static readonly Guid TestStudentId = MultitenancyEfJobRunner.TestStudentId;

    [Fact]
    public async Task HttpMaster_InsertsOrUpdatesTenant_AndPersists()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var uowMaster = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
            var repoTenant = await uowMaster.GetRepositoryAsync<IRepository<Tenant>>();

            var beforeGet = await repoTenant.GetQuery().ToListAsync();
            var connectionStamp = "Test Tenant " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var tenant = await repoTenant.GetByIdAsync(x => x.Id == MasterTenantId);
            if (tenant == null)
            {
                tenant = await repoTenant.InsertAsync(new Tenant
                {
                    Id = MasterTenantId,
                    ConnectionString = connectionStamp,
                });
            }
            else
            {
                tenant.ConnectionString = connectionStamp;
            }

            await uowMaster.SaveChangesAsync();

            var afterGet = await repoTenant.GetQuery().ToListAsync();

            Assert.Contains(afterGet, t => t.Id == MasterTenantId);
            Assert.Equal(connectionStamp, tenant.ConnectionString);
            Assert.True(afterGet.Count >= beforeGet.Count);
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    [Fact]
    public async Task HttpMasterTenant_QueriesMasterAndTenantDb_AfterSwitchDbContext()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await MultitenancyEfTestDataSeeder.EnsureTenantRegisteredAsync(provider, MasterTenantId);

            await using var scope = provider.CreateAsyncScope();
            var uowMaster = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
            var uowTenant = scope.ServiceProvider.GetRequiredService<ISampleUnitOfWork>();

            await uowTenant.SwitchDbContextAsync(MasterTenantId);

            var repoTenant = await uowMaster.GetRepositoryAsync<IRepository<Tenant>>();
            var tenants = await repoTenant.GetQuery().ToListAsync();

            var repoStudent = await uowTenant.GetRepositoryAsync<IRepository<Student>>();
            var beforeGet = await repoStudent.GetQuery().ToListAsync();

            var student = await repoStudent.GetByIdAsync(x => x.Id == TestStudentId);
            if (student == null)
            {
                student = await repoStudent.InsertAsync(new Student
                {
                    Id = TestStudentId,
                    Name = "Test Student",
                    TenantId = MasterTenantId,
                });
            }
            else
            {
                student.Name = "Test Student " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }

            await uowTenant.SaveChangesAsync();

            var afterGet = await repoStudent.GetQuery().ToListAsync();

            Assert.Contains(tenants, t => t.Id == MasterTenantId);
            Assert.Contains(afterGet, s => s.Id == TestStudentId && s.TenantId == MasterTenantId);
            Assert.True(afterGet.Count >= beforeGet.Count);
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    [Fact]
    public async Task JobMasterWithoutTenant_InsertsOrUpdatesTenantInMaster()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredService<MultitenancyEfJobRunner>();

            var result = await runner.RunMasterWithoutTenantAsync(scope.ServiceProvider);

            Assert.Equal(MasterTenantId, result.TenantId);
            Assert.False(string.IsNullOrWhiteSpace(result.ConnectionString));
            Assert.True(result.AfterCount >= result.BeforeCount);
            Assert.True(result.AfterCount >= 1);
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    [Fact]
    public async Task JobMasterTenantWithTenantId_InsertsOrUpdatesStudentInTenantDb()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await MultitenancyEfTestDataSeeder.EnsureTenantRegisteredAsync(provider, MasterTenantId);

            await using var scope = provider.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredService<MultitenancyEfJobRunner>();

            var result = await runner.RunMasterTenantWithTenantIdAsync(
                scope.ServiceProvider,
                MasterTenantId);

            Assert.Equal(MasterTenantId, result.TenantId);
            Assert.Equal(TestStudentId, result.StudentId);
            Assert.False(string.IsNullOrWhiteSpace(result.StudentName));
            Assert.True(result.AfterCount >= result.BeforeCount);
            Assert.True(result.AfterCount >= 1);
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    [Fact]
    public async Task TenantDb_IsolatedPerTenant_WhenUsingSeparateInMemoryDatabases()
    {
        var tenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var tenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await MultitenancyEfTestDataSeeder.EnsureTenantRegisteredAsync(provider, tenantA);
            await MultitenancyEfTestDataSeeder.EnsureTenantRegisteredAsync(provider, tenantB);

            await using (var scopeA = provider.CreateAsyncScope())
            {
                var uow = scopeA.ServiceProvider.GetRequiredService<ISampleUnitOfWork>();
                await uow.SwitchDbContextAsync(tenantA);
                var repo = await uow.GetRepositoryAsync<IRepository<Student>>();
                await repo.InsertAsync(new Student
                {
                    Id = Guid.NewGuid(),
                    Name = "Student A",
                    TenantId = tenantA,
                });
                await uow.SaveChangesAsync();
            }

            await using (var scopeB = provider.CreateAsyncScope())
            {
                var uow = scopeB.ServiceProvider.GetRequiredService<ISampleUnitOfWork>();
                await uow.SwitchDbContextAsync(tenantB);
                var repo = await uow.GetRepositoryAsync<IRepository<Student>>();
                var count = await repo.GetQuery().CountAsync();
                Assert.Equal(0, count);
            }

            await using (var scopeA2 = provider.CreateAsyncScope())
            {
                var uow = scopeA2.ServiceProvider.GetRequiredService<ISampleUnitOfWork>();
                await uow.SwitchDbContextAsync(tenantA);
                var repo = await uow.GetRepositoryAsync<IRepository<Student>>();
                var count = await repo.GetQuery().CountAsync();
                Assert.Equal(1, count);
            }
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    [Fact]
    public async Task TenantDb_SaveChanges_WithoutTenantId_Throws()
    {
        var root = new InMemoryDatabaseRoot();
        var provider = MultitenancyEfTestServiceFactory.Create(root);
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TenantDbContext>>();
            await using var context = await factory.CreateDbContextAsync();

            context.Students.Add(new Student
            {
                Id = Guid.NewGuid(),
                Name = "No tenant",
                TenantId = Guid.NewGuid(),
            });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("TenantId is required", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            await DisposeProviderAsync(provider);
        }
    }

    private static async Task DisposeProviderAsync(IServiceProvider provider)
    {
        if (provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (provider is IDisposable disposable)
            disposable.Dispose();
    }
}
