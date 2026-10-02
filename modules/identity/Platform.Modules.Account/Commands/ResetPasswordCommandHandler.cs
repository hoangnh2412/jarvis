namespace Platform.Modules.Account.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Commands;

public sealed class ResetPasswordCommandHandler : IAsyncCommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid token or email");

        var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to reset password: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
