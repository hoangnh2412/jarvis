using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Account.Abstractions;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/platform/account/catalog")]
public sealed class PlatformLoginCatalogController(IPlatformLoginCatalogService catalog) : ControllerBase
{
    [HttpGet("tenants")]
    public async Task<IActionResult> ListTenants(CancellationToken cancellationToken)
    {
        var items = await catalog.ListActiveTenantsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("tenants/{tenantId:guid}/orgs")]
    public async Task<IActionResult> ListOrgs(Guid tenantId, CancellationToken cancellationToken)
    {
        var items = await catalog.ListActiveOrgsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (items is null)
            return NotFound();

        return Ok(items);
    }
}
