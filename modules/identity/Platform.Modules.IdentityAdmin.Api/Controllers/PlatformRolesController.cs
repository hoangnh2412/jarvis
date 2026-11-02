using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;
using Platform.Modules.IdentityAdmin.Api.Models;
using Platform.Modules.IdentityAdmin.Commands;
using Platform.Modules.IdentityAdmin.Queries;
using Platform.Tenants;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[Authorize(Policy = TenantPermissions.View)]
[Route("api/v1/platform/roles")]
public sealed class PlatformRolesController(
    IAsyncCommandDispatcher commands,
    IAsyncQueryDispatcher queries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await queries.DispatchAsync<GetRolesQuery, IReadOnlyCollection<RoleSummaryDto>>(new GetRolesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.DispatchAsync<GetRoleQuery, RoleDetailDto>(new GetRoleQuery { RoleId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> Create([FromBody] IdentityRoleWrite input, CancellationToken cancellationToken)
    {
        var result = await commands.DispatchAsync<CreateRoleCommand, CreateRoleResponse>(new CreateRoleCommand
        {
            Name = input.Name,
            DisplayName = input.DisplayName,
            RoleLevel = input.RoleLevel,
            IsActive = input.IsActive,
        }, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] IdentityRoleWrite input, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new UpdateRoleCommand
        {
            RoleId = id,
            Name = input.Name,
            DisplayName = input.DisplayName,
            RoleLevel = input.RoleLevel,
            IsActive = input.IsActive,
        }, cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] IdentityRolePermissionsWrite input, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new SetRolePermissionsCommand
        {
            RoleId = id,
            Permissions = input.Permissions,
        }, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = TenantPermissions.Manage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new DeleteRoleCommand { RoleId = id }, cancellationToken);
        return Ok();
    }
}
