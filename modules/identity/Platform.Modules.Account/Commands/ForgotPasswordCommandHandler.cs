namespace Platform.Modules.Account.Commands;

using Platform.Modules.Account.Abstractions;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Commands;

public sealed class ForgotPasswordCommandHandler : IAsyncCommandHandler<ForgotPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(
        UserManager<User> userManager,
        IEmailSender emailSender)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task HandleAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return;

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailSender.SendPasswordResetEmailAsync(command.Email, resetToken, cancellationToken);
    }
}
