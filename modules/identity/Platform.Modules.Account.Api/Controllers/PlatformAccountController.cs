using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.Modules.Account.Commands;

namespace Platform.Modules.Account.Api.Controllers;

[ApiController]
[Route("api/v1/platform/account")]
public sealed class PlatformAccountController(
    IAsyncCommandHandler<LoginCommand, LoginResponse> login,
    IAsyncCommandDispatcher commands) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login([FromBody] LoginBody body, CancellationToken cancellationToken) =>
        AccountLogin.ExecuteAsync(login, body.UserName, body.Password, AccountLogin.PlatformRealm, cancellationToken);

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

    public sealed record LoginBody(string UserName, string Password);

    public sealed record ChangePasswordBody(string CurrentPassword, string NewPassword);
}
