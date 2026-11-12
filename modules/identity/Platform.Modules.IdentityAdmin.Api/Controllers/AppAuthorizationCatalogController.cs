using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Abstractions;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/app/authorization")]
public sealed class AppAuthorizationCatalogController(
    IPermissionProvider permissions,
    IPermissionCatalogProvider catalog,
    IdentityDbContextBase identityDb) : ControllerBase
{
    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken)
    {
        var policyRows = await identityDb.Policies
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new { p.Code, p.Name, p.IsSystemPolicy })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            permissions = catalog.GetAppRoleCatalogPermissions().Select(permission => new
            {
                permission.Code,
                permission.Module,
                permission.Group,
                permission.DisplayName,
            }),
            policies = policyRows,
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var userId))
            return Unauthorized();

        var granted = await permissions.GetPermissionsAsync(userId, cancellationToken).ConfigureAwait(false);
        if (granted.Any(catalog.IsFullAccessPermission))
        {
            foreach (var descriptor in catalog.GetAllPermissions())
                granted.Add(descriptor.Code);
        }

        return Ok(granted.OrderBy(code => code, StringComparer.Ordinal).ToArray());
    }
}
