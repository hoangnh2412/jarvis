using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Application.Contracts.Commands;
using Platform.Modules.Account.Commands;

namespace Platform.Modules.Account.Api.Controllers;

public static class AccountLogin
{
    public const string AppRealm = "app";

    public const string PlatformRealm = "platform";

    public static async Task<IActionResult> ExecuteAsync(
        IAsyncCommandHandler<LoginCommand, LoginResponse> login,
        string? userName,
        string? password,
        string realm,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return new BadRequestObjectResult(new { message = "Thiếu tên đăng nhập hoặc mật khẩu." });

        try
        {
            var result = await login.HandleAsync(
                new LoginCommand
                {
                    Username = userName.Trim(),
                    Password = password,
                    Realm = realm,
                },
                cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }
        catch (UnauthorizedAccessException)
        {
            return new UnauthorizedResult();
        }
    }
}
