using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Account.Abstractions;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class MeV1Controller(IAccountMeService accountMe) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out _))
            return Unauthorized();

        var payload = await accountMe.GetMeAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            account = new
            {
                id = payload.Account.Id,
                userName = payload.Account.UserName,
                displayName = payload.Account.DisplayName,
                email = payload.Account.Email,
            },
            permissions = payload.Permissions,
            roles = payload.Roles,
            organizationalUnit = payload.OrganizationalUnit is null
                ? null
                : new
                {
                    id = payload.OrganizationalUnit.Id,
                    code = payload.OrganizationalUnit.Code,
                    name = payload.OrganizationalUnit.Name,
                    unitType = payload.OrganizationalUnit.UnitType,
                    path = payload.OrganizationalUnit.Path,
                },
            descendants = payload.Descendants.Select(unit => new
            {
                id = unit.Id,
                code = unit.Code,
                name = unit.Name,
                unitType = unit.UnitType,
                path = unit.Path,
            }),
        });
    }
}
