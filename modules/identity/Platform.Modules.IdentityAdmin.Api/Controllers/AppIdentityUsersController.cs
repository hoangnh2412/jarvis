using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Api.Models;
using Platform.Modules.IdentityAdmin.Commands;
using Platform.Modules.IdentityAdmin.Queries;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/app/users")]
public sealed class AppIdentityUsersController(
    IAsyncCommandDispatcher commands,
    IAsyncQueryDispatcher queries,
    IUserOrgScopeAdminService? orgScope = null) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await queries.DispatchAsync<GetUsersQuery, IReadOnlyCollection<UserSummaryDto>>(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.DispatchAsync<GetUserQuery, UserDetailDto>(new GetUserQuery { UserId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IdentityUserCreateWrite input, CancellationToken cancellationToken)
    {
        var result = await commands.DispatchAsync<CreateUserCommand, CreateUserResponse>(new CreateUserCommand
        {
            Username = input.Username,
            Email = input.Email,
            Password = input.Password,
            PhoneNumber = input.PhoneNumber,
            DisplayName = input.DisplayName,
            RoleIds = input.RoleIds,
        }, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] IdentityUserUpdateWrite input, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new UpdateUserAccountCommand
        {
            UserId = id,
            Username = input.Username,
            Email = input.Email,
            PhoneNumber = input.PhoneNumber,
            DisplayName = input.DisplayName,
            Password = input.Password,
            IsLocked = input.IsLocked,
            RoleIds = input.RoleIds,
        }, cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, [FromBody] IdentityUserLockWrite input, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new UpdateUserCommand
        {
            UserId = id,
            IsLocked = input.IsLocked,
        }, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new DeleteUserCommand { UserId = id }, cancellationToken);
        return Ok();
    }

    [HttpPut("{userId:guid}/scope")]
    public async Task<IActionResult> SetUserScope(
        Guid userId,
        [FromBody] IdentityUserScopeWrite input,
        CancellationToken cancellationToken)
    {
        if (orgScope is null)
            return NotFound();

        var outcome = await orgScope.SetUserScopeAsync(userId, input.OrganizationalUnitId, cancellationToken)
            .ConfigureAwait(false);

        return outcome.Status switch
        {
            UserOrgScopeUpdateStatus.ScopeMissing => StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Thiếu tenant hoặc tổ chức đang làm việc." }),
            UserOrgScopeUpdateStatus.NotFound => NotFound(),
            _ => Ok(outcome.Data),
        };
    }
}
