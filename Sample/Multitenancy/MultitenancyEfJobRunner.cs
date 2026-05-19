using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Multitenancy;

/// <summary>
/// Background-style EF multitenancy tests (same logic as <c>MultitenancyEfTestController</c> job endpoints).
/// </summary>
public sealed class MultitenancyEfJobRunner(ILogger<MultitenancyEfJobRunner> logger)
{
    public static readonly Guid MasterTenantRegistryId = Guid.Parse("dd348565-5d31-46bb-aef7-aa7f0c4a1866");
    public static readonly Guid TestStudentId = Guid.Parse("006a88c9-0286-47b0-ac87-91e53c0bf2ba");

    /// <summary>Job without ambient tenant: Master DB only.</summary>
    public async Task<MasterJobResult> RunMasterWithoutTenantAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var uow = serviceProvider.GetRequiredService<IMasterUnitOfWork>();
        var repoTenant = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken).ConfigureAwait(false);
        var beforeCount = await repoTenant.GetQuery().CountAsync(cancellationToken).ConfigureAwait(false);

        var tenant = await repoTenant.GetByIdAsync(x => x.Id == MasterTenantRegistryId, cancellationToken).ConfigureAwait(false);
        if (tenant == null)
        {
            tenant = await repoTenant.InsertAsync(new Tenant
            {
                Id = MasterTenantRegistryId,
                ConnectionString = "Test Tenant " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            }, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Master job: inserted tenant {TenantId}", MasterTenantRegistryId);
        }
        else
        {
            tenant.ConnectionString = "Test Tenant " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logger.LogInformation("Master job: updated tenant {TenantId}", MasterTenantRegistryId);
        }

        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var afterCount = await repoTenant.GetQuery().CountAsync(cancellationToken).ConfigureAwait(false);

        return new MasterJobResult(beforeCount, afterCount, tenant.Id, tenant.ConnectionString);
    }

    /// <summary>Job with explicit tenant: ambient tenant via <see cref="ICurrentTenantAccessor"/> then tenant DB.</summary>
    public async Task<TenantJobResult> RunMasterTenantWithTenantIdAsync(
        IServiceProvider serviceProvider,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var uow = serviceProvider.GetRequiredService<ISampleUnitOfWork>();
        await uow.SwitchDbContextAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var repoStudent = await uow.GetRepositoryAsync<IRepository<Student>>(cancellationToken).ConfigureAwait(false);
        // var dbContext = (TenantDbContext)await uow.GetDbContextAsync(cancellationToken).ConfigureAwait(false);
        // var database = await GetDatabaseNameAsync(dbContext, cancellationToken).ConfigureAwait(false);
        // Console.WriteLine($"TenantId: {dbContext.TenantId}");
        var beforeCount = await repoStudent.GetQuery().CountAsync(cancellationToken);

        var student = await repoStudent
            .GetByIdAsync(x => x.Id == TestStudentId, cancellationToken)
            .ConfigureAwait(false);
        if (student == null)
        {
            student = await repoStudent.InsertAsync(new Student
            {
                Id = TestStudentId,
                Name = "Test Student",
                TenantId = tenantId,
            }, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Tenant job: inserted student {StudentId} for tenant {TenantId}", TestStudentId, tenantId);
        }
        else
        {
            student.Name = "Test Student " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logger.LogInformation("Tenant job: updated student {StudentId} for tenant {TenantId}", TestStudentId, tenantId);
        }

        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var afterCount = await repoStudent.GetQuery().CountAsync(cancellationToken).ConfigureAwait(false);

        return new TenantJobResult(
            tenantId,
            beforeCount,
            afterCount,
            student.Id,
            student.Name);
    }

    private static async Task<string?> GetDatabaseNameAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        return connection.Database;
    }
}

public sealed record MasterJobResult(int BeforeCount, int AfterCount, Guid TenantId, string ConnectionString);

public sealed record TenantJobResult(
    Guid TenantId,
    int BeforeCount,
    int AfterCount,
    Guid StudentId,
    string StudentName);
