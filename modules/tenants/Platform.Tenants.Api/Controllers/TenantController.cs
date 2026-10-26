using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Tenants;
using Platform.Tenants.Api.Models;
using Platform.Tenants.Models;
using Platform.Tenants.Services;

namespace Platform.Tenants.Api.Controllers;

/// <summary>
/// API vận hành catalog tenant. Route <c>/platform/tenants</c>.
/// Host gắn policy <see cref="TenantPermissions.View"/> và <see cref="TenantPermissions.Manage"/> cho user root.
/// Request thiếu header tenant không được coi là root.
/// </summary>
[Route("platform/tenants")]
[ApiController]
[Authorize(Policy = TenantPermissions.View)]
public sealed class TenantController(
    ITenantManager tenants,
    IPlatformCatalogAdminProvisioner? adminProvisioner = null) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var items = await tenants
            .ListAsync(new ListTenantsQuery { IsActive = isActive, Search = search }, cancellationToken)
            .ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpGet("{id:guid}/subtree")]
    public async Task<IActionResult> ListSubtreeAsync(Guid id, CancellationToken cancellationToken)
    {
        var items = await tenants.ListSubtreeAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("{id:guid}/connection")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> GetConnectionStringAsync(Guid id, CancellationToken cancellationToken)
    {
        var connectionString = await tenants.GetConnectionStringAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok(new { connectionString });
    }

    [HttpPost]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTenantHttpRequest request,
        CancellationToken cancellationToken)
    {
        var created = await tenants.CreateAsync(new CreateTenantInput
        {
            Code = request.Code,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            ParentTenantId = request.ParentTenantId,
            InitialAdmin = request.InitialAdmin,
        }, cancellationToken).ConfigureAwait(false);

        if (request.InitialAdmin is not null && adminProvisioner is not null)
        {
            var admin = await adminProvisioner
                .ProvisionTenantAdminAsync(created.Id, request.InitialAdmin, cancellationToken)
                .ConfigureAwait(false);
            return Ok(new { tenant = created, initialAdmin = admin });
        }

        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateTenantHttpRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await tenants.UpdateAsync(id, new UpdateTenantInput
        {
            Code = request.Code,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
        }, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> SetActiveAsync(
        Guid id,
        [FromBody] SetTenantActiveHttpRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await tenants.SetActiveAsync(id, request.IsActive, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPut("{id:guid}/parent")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> SetParentAsync(
        Guid id,
        [FromBody] SetTenantParentHttpRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await tenants.SetParentAsync(id, request.ParentTenantId, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }
}
