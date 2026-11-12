using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.Modules.Account.Abstractions;
using Platform.Modules.Account.Commands;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/app/account")]
public sealed class AppAccountController(
    IAsyncCommandHandler<AppLoginCommand, AppLoginResult> login,
    IAsyncCommandHandler<AppRefreshTokenCommand, AppLoginResult> refresh,
    IAsyncCommandDispatcher commands) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AppLoginBody body, CancellationToken cancellationToken)
    {
        var result = await login.HandleAsync(
                new AppLoginCommand
                {
                    TenantCode = body.TenantCode ?? string.Empty,
                    OrgCode = body.OrgCode ?? string.Empty,
                    UserName = body.UserName,
                    Password = body.Password,
                },
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            userId = result.UserId,
            tenantId = result.TenantId,
            orgId = result.OrgId,
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] AppRefreshBody body, CancellationToken cancellationToken)
    {
        var result = await refresh.HandleAsync(
                new AppRefreshTokenCommand { RefreshToken = body.RefreshToken ?? string.Empty },
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            userId = result.UserId,
            tenantId = result.TenantId,
            orgId = result.OrgId,
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(new LogoutCommand(), cancellationToken).ConfigureAwait(false);
        return Ok();
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] AppChangePasswordBody body, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(
            new ChangePasswordCommand
            {
                CurrentPassword = body.CurrentPassword,
                NewPassword = body.NewPassword,
            },
            cancellationToken).ConfigureAwait(false);
        return Ok();
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] AppUpdateProfileBody body, CancellationToken cancellationToken)
    {
        await commands.DispatchAsync(
            new UpdateProfileCommand
            {
                Email = body.Email,
                PhoneNumber = body.PhoneNumber,
            },
            cancellationToken).ConfigureAwait(false);
        return Ok();
    }

    public sealed record AppLoginBody(string UserName, string Password, string? TenantCode, string? OrgCode);

    public sealed record AppRefreshBody(string? RefreshToken);

    public sealed record AppChangePasswordBody(string CurrentPassword, string NewPassword);

    public sealed record AppUpdateProfileBody(string? Email, string? PhoneNumber);
}
