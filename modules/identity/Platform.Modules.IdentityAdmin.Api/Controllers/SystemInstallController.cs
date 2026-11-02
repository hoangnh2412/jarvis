using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.IdentityAdmin.Abstractions;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/system/install")]
public sealed class SystemInstallController(ISystemInstallService install) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Install([FromBody] InstallBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.User?.UserName)
            || string.IsNullOrWhiteSpace(body.User.Password))
        {
            return BadRequest(new { message = "Thiếu tên đăng nhập hoặc mật khẩu." });
        }

        var outcome = await install.InstallAsync(
            new IdentityInstallRequest(
                body.User.UserName,
                body.User.Password,
                body.User.DisplayName),
            cancellationToken).ConfigureAwait(false);

        return outcome.Status switch
        {
            IdentityInstallStatus.Created => Created("/api/v1/platform/account/login", new
            {
                userId = outcome.UserId,
            }),
            IdentityInstallStatus.AlreadyInstalled => Conflict(new { message = "Hệ thống đã được cài đặt." }),
            IdentityInstallStatus.InvalidPassword => BadRequest(new { message = "Mật khẩu không đạt chính sách.", errors = outcome.Errors }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    public sealed record InstallBody(TenantBody? Tenant, UserBody? User);

    public sealed record TenantBody(string Code, string Name);

    public sealed record UserBody(string UserName, string Password, string? DisplayName);
}
