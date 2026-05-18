using Asp.Versioning;
using Jarvis.Domain.DataStorages;
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
[Route("api/v{version:apiVersion}/tests/ef")]
public class MultitenancyEfTestController(
    ISampleUnitOfWork unitOfWork,
    IMasterUnitOfWork masterUnitOfWork,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ITenantConnectionStringResolverFactory connectionStringFactory,
    TenantEfTestService tenantEfTestService) : ControllerBase
{
    /// <summary>
    /// Case 1: send <c>X-Tenant-Id: {tenant-guid}</c>. Interceptor resolves connection from Master.Tenant when tenant exists.
    /// </summary>
    [HttpGet("http-tenant")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<TenantEfTestResult>> GetWithHeaderAsync(CancellationToken cancellationToken)
    {
        var result = await TenantEfTestService.RunHttpRequestAsync(
            unitOfWork,
            tenantIdResolverFactory,
            connectionStringFactory,
            cancellationToken).ConfigureAwait(false);

        return Ok(result);
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

    /// <summary>
    /// Lists tenants from Master DB (connection string registry).
    /// </summary>
    [HttpGet("master/tenants")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<IReadOnlyList<object>>> ListMasterTenantsAsync(CancellationToken cancellationToken)
    {
        var masterContext = (MasterDbContext)await masterUnitOfWork.GetDbContextAsync(cancellationToken).ConfigureAwait(false);
        var tenants = await masterContext.Tenants
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                ConnectionStringPreview = ConnectionStringHelper.Mask(t.ConnectionString)
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Ok(tenants);
    }

    /// <summary>
    /// Seeds a tenant row in Master (for local testing).
    /// </summary>
    [HttpPost("master/tenants")]
    [MapToApiVersion(2.0)]
    public async Task<ActionResult<object>> SeedMasterTenantAsync(
        [FromBody] Tenant seed,
        CancellationToken cancellationToken)
    {
        var masterContext = (MasterDbContext)await masterUnitOfWork.GetDbContextAsync(cancellationToken).ConfigureAwait(false);
        masterContext.Tenants.Add(seed);
        await masterUnitOfWork.SaveAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            seed.Id,
            ConnectionStringPreview = ConnectionStringHelper.Mask(seed.ConnectionString)
        });
    }
}
