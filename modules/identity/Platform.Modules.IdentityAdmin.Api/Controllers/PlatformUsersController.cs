using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Api.Models;
using Platform.Tenants;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = TenantPermissions.View)]
[Route("api/v{version:apiVersion}/platform/users")]
public sealed class PlatformUsersController(IPlatformUserAdminService users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var items = await users.ListAsync(search, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await users.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (item is null)
            throw new NotFoundException(IdentityAdminErrorCode.PlatformUserNotFound);

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePlatformUserRequest request,
        CancellationToken cancellationToken)
    {
        var created = await users.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdatePlatformUserRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await users.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPut("{id:guid}/lock")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> LockAsync(
        Guid id,
        [FromBody] IdentityUserLockWrite request,
        CancellationToken cancellationToken)
    {
        var updated = await users.SetLockedAsync(id, request.IsLocked, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await users.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok();
    }
}
