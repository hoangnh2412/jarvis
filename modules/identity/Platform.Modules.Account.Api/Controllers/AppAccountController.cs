using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.Modules.Account.Abstractions;
using Platform.Modules.Account.Commands;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[Route("api/v1/app/account")]
public sealed class AppAccountController(
    IAppLoginService login,
    IAsyncCommandDispatcher commands) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.UserName)
            || string.IsNullOrWhiteSpace(body.Password)
            || string.IsNullOrWhiteSpace(body.TenantCode)
            || string.IsNullOrWhiteSpace(body.OrgCode))
        {
            return BadRequest(new { message = "Thiếu mã tenant, mã organization, tên đăng nhập hoặc mật khẩu." });
        }

        try
        {
            var result = await login.LoginAsync(
                    body.TenantCode,
                    body.OrgCode,
                    body.UserName,
                    body.Password,
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
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.RefreshToken))
            return Unauthorized();

        try
        {
            var result = await login.RefreshAsync(body.RefreshToken, cancellationToken).ConfigureAwait(false);
            return Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                userId = result.UserId,
                tenantId = result.TenantId,
                orgId = result.OrgId,
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordBody body, CancellationToken cancellationToken)
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
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileBody body, CancellationToken cancellationToken)
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

    public sealed record LoginBody(string UserName, string Password, string? TenantCode, string? OrgCode);

    public sealed record RefreshBody(string? RefreshToken);

    public sealed record ChangePasswordBody(string CurrentPassword, string NewPassword);

    public sealed record UpdateProfileBody(string? Email, string? PhoneNumber);
}
