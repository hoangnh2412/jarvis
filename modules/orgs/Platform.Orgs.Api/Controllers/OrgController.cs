using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Orgs.Api.Models;
using Platform.Orgs.Models;
using Platform.Orgs.Services;
using Platform.Tenants.Services;

namespace Platform.Orgs.Api.Controllers;

/// <summary>
/// HTTP API admin catalog Org. Tenant qua <c>ICurrentTenant</c>.
/// Permission gợi ý: <see cref="OrgPermissions.View"/> / <see cref="OrgPermissions.Manage"/> — host map sang policy hoặc filter riêng.
/// </summary>
[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/orgs")]
[ApiController]
[Authorize]
public sealed class OrgController(
    IOrgManager orgManager,
    IPlatformCatalogAdminProvisioner? adminProvisioner = null) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var items = await orgManager
            .ListAsync(new ListOrgsQuery { IsActive = isActive, Search = search }, cancellationToken)
            .ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var org = await orgManager.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return org is null ? NotFound() : Ok(org);
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var org = await orgManager.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false);
        return org is null ? NotFound() : Ok(org);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOrgHttpRequest request,
        CancellationToken cancellationToken)
    {
        var created = await orgManager
            .CreateAsync(new CreateOrgInput
            {
                Code = request.Code,
                Name = request.Name,
                InitialAdmin = request.InitialAdmin,
            }, cancellationToken)
            .ConfigureAwait(false);

        if (request.InitialAdmin is not null && adminProvisioner is not null)
        {
            var admin = await adminProvisioner
                .ProvisionOrgAdminAsync(created.TenantId, created.Id, request.InitialAdmin, cancellationToken)
                .ConfigureAwait(false);
            return Ok(new { org = created, initialAdmin = admin });
        }

        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateOrgHttpRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await orgManager
            .UpdateAsync(id, new UpdateOrgInput { Code = request.Code, Name = request.Name }, cancellationToken)
            .ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActiveAsync(
        Guid id,
        [FromBody] SetOrgActiveHttpRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await orgManager.SetActiveAsync(id, request.IsActive, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }
}
