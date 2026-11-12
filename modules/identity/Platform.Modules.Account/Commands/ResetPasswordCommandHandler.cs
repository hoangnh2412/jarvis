namespace Platform.Modules.Account.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Shared.ExceptionHandling;

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
            throw new BusinessException(AccountErrorCode.ResetPasswordInvalid);

        var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);
        if (!result.Succeeded)
            throw new BusinessException(
                AccountErrorCode.ResetPasswordFailed,
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
