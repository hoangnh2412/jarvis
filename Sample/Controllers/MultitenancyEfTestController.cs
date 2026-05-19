using Asp.Versioning;
using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;
using Sample.Multitenancy;
using Sample.Persistence;

namespace Sample.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/tests/ef-multitenancy")]
public class MultitenancyEfTestController : ControllerBase
{
    [HttpGet("http/master")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> ListMasterTenantsAsync(
        [FromServices] IMasterUnitOfWork uowMaster,
        CancellationToken cancellationToken)
    {
        var repoTenant = await uowMaster.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken);
        var beforeGet = await repoTenant
            .GetQuery()
            .ToListAsync(cancellationToken);

        var tenant = await repoTenant.GetByIdAsync(x => x.Id == Guid.Parse("dd348565-5d31-46bb-aef7-aa7f0c4a1866"), cancellationToken);
        if (tenant == null)
        {
            tenant = await repoTenant.InsertAsync(new Tenant
            {
                Id = Guid.Parse("dd348565-5d31-46bb-aef7-aa7f0c4a1866"),
                ConnectionString = "Test Tenant " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }, cancellationToken);
        }
        else
        {
            tenant.ConnectionString = "Test Tenant " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        await uowMaster.SaveChangesAsync(cancellationToken);

        var afterGet = await repoTenant
            .GetQuery()
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Before = beforeGet,
            Tenant = tenant,
            After = afterGet
        });
    }

    [HttpGet("http/master-tenant")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> QueryMasterDbAsync(
        [FromServices] IMasterUnitOfWork uowMaster,
        [FromServices] ISampleUnitOfWork uowTenant,
        [FromServices] ITenantIdResolverFactory tenantIdResolverFactory,
        CancellationToken cancellationToken)
    {
        var tenantId = await tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (!tenantId.HasValue)
            throw new Exception("Send header X-Tenant-Id with the tenant Guid registered in Master.Tenant.");

        await uowTenant.SwitchDbContextAsync(tenantId.Value, cancellationToken).ConfigureAwait(false);

        var repoTenant = await uowMaster.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken);
        var tenants = await repoTenant.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        var repoStudent = await uowTenant.GetRepositoryAsync<IRepository<Student>>(cancellationToken);
        var beforeGet = await repoStudent.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        var studentId = Guid.Parse("006a88c9-0286-47b0-ac87-91e53c0bf2ba");
        var student = await repoStudent.GetByIdAsync(x => x.Id == studentId, cancellationToken);
        if (student == null)
        {
            student = await repoStudent.InsertAsync(new Student
            {
                Id = studentId,
                Name = "Test Student",
                TenantId = tenantId.Value,
            }, cancellationToken);
        }
        else
        {
            student.Name = "Test Student " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        await uowTenant.SaveChangesAsync(cancellationToken);

        var afterGet = await repoStudent.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        return Ok(new
        {
            Tenants = tenants,
            Before = beforeGet,
            Student = student,
            After = afterGet
        });
    }

    [HttpGet("job/master-tenant")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> RunBackgroundJobWithTenantIdAsync(
        [FromServices] IServiceScopeFactory scopeFactory,
        [FromServices] MultitenancyEfJobRunner jobRunner,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var result = await jobRunner
            .RunMasterTenantWithTenantIdAsync(scope.ServiceProvider, tenantId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    [HttpGet("job/master")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> RunBackgroundJobWithoutTenantIdAsync(
        [FromServices] IServiceScopeFactory scopeFactory,
        [FromServices] MultitenancyEfJobRunner jobRunner,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var result = await jobRunner
            .RunMasterWithoutTenantAsync(scope.ServiceProvider, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
