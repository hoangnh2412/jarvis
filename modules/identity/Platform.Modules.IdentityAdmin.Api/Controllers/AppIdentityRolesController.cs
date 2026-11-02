using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;
using Platform.Modules.IdentityAdmin.Api.Models;
using Platform.Modules.IdentityAdmin.Commands;
using Platform.Modules.IdentityAdmin.Queries;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/app/roles")]
public sealed class AppIdentityRolesController(
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
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] IdentityRolePermissionsWrite input, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new SetRolePermissionsCommand
        {
            RoleId = id,
            Permissions = input.Permissions,
        }, cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/policies")]
    public async Task<IActionResult> SetPolicies(Guid id, [FromBody] IdentityRolePoliciesWrite input, CancellationToken cancellationToken)
    {
        var role = await queries.DispatchAsync<GetRoleQuery, RoleDetailDto>(new GetRoleQuery { RoleId = id }, cancellationToken);
        var desired = (input.Policies ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var current = role.Policies ?? [];

        foreach (var code in current.Except(desired, StringComparer.Ordinal))
        {
            await commands.DispatchAsync(new RemovePolicyFromRoleCommand
            {
                RoleId = id,
                PolicyCode = code,
            }, cancellationToken);
        }

        foreach (var code in desired.Except(current, StringComparer.Ordinal))
        {
            await commands.DispatchAsync(new AssignPolicyToRoleCommand
            {
                RoleId = id,
                PolicyCode = code,
            }, cancellationToken);
        }

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new DeleteRoleCommand { RoleId = id }, cancellationToken);
        return Ok();
    }
}
