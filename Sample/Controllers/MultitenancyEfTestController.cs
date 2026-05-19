using Asp.Versioning;
using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;
using Sample.Multitenancy;
using Sample.Persistence;

namespace Sample.Controllers;

/// <summary>
/// Demo EF multitenancy: HTTP header vs background job.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/tests/ef-multitenancy")]
public class MultitenancyEfTestController(
    // ITenantIdResolverFactory tenantIdResolverFactory,
    // ITenantConnectionStringResolverFactory connectionStringFactory,
    TenantEfTestService tenantEfTestService) : ControllerBase
{
    /// <summary>
    /// Lists tenants from Master DB (connection string registry).
    /// </summary>
    [HttpGet("http/master")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<IReadOnlyList<object>>> ListMasterTenantsAsync(
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
                ConnectionString = Guid.NewGuid().ToString()
            }, cancellationToken);
        }
        else
        {
            tenant.ConnectionString = Guid.NewGuid().ToString();
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

    /// <summary>
    /// Case 1: send <c>X-Tenant-Id: {tenant-guid}</c>. Interceptor resolves connection from Master.Tenant when tenant exists.
    /// </summary>
    [HttpGet("http/master-tenant")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<TenantEfTestResult>> QueryMasterDbAsync(
        [FromServices] IMasterUnitOfWork uowMaster,
        [FromServices] ISampleUnitOfWork uowTenant,
        CancellationToken cancellationToken)
    {
        var repoTenant = await uowMaster.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken);
        var tenants = await repoTenant.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        var repoStudent = await uowTenant.GetRepositoryAsync<IRepository<Student>>(cancellationToken);
        var beforeGet = await repoStudent.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        var afterGet = await repoStudent.GetQuery().ToListAsync(cancellationToken: cancellationToken);

        return Ok(new
        {
            Tenants = tenants,
            Before = beforeGet,
            After = afterGet
        });
    }

    /// <summary>
    /// Case 2: simulate background job with explicit tenant id (no HTTP context).
    /// </summary>
    [HttpPost("background-job")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<TenantEfTestResult>> RunBackgroundJobAsync(
        [FromBody] BackgroundJobTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await tenantEfTestService
            .RunBackgroundJobAsync(request.TenantId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
