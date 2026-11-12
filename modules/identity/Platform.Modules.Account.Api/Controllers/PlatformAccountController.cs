using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.Modules.Account.Commands;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/account")]
public sealed class PlatformAccountController(
    IAsyncCommandHandler<PlatformLoginCommand, LoginResponse> login,
    IAsyncCommandDispatcher commands) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] PlatformLoginBody body, CancellationToken cancellationToken)
    {
        var result = await login.HandleAsync(
            new PlatformLoginCommand
            {
                Username = body.UserName,
                Password = body.Password,
            },
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
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
    public async Task<IActionResult> ChangePassword([FromBody] PlatformChangePasswordBody body, CancellationToken cancellationToken)
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

    public sealed record PlatformLoginBody(string UserName, string Password);

    public sealed record PlatformChangePasswordBody(string CurrentPassword, string NewPassword);
}
