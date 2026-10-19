namespace Platform.Modules.Account.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;

public sealed class UpdateProfileCommandHandler : IAsyncCommandHandler<UpdateProfileCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public UpdateProfileCommandHandler(
        UserManager<User> userManager,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var user = await _userManager.FindByIdAsync(actor.UserId.Value.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (command.Email != null && !string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _userManager.SetEmailAsync(user, command.Email);
            if (!emailResult.Succeeded)
                throw new InvalidOperationException($"Failed to update profile: {string.Join("; ", emailResult.Errors.Select(e => e.Description))}");

            user.EmailConfirmed = false;
            var confirmResult = await _userManager.UpdateAsync(user);
            if (!confirmResult.Succeeded)
                throw new InvalidOperationException($"Failed to update profile: {string.Join("; ", confirmResult.Errors.Select(e => e.Description))}");
        }

        if (command.PhoneNumber != null && user.PhoneNumber != command.PhoneNumber)
        {
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, command.PhoneNumber);
            if (!phoneResult.Succeeded)
                throw new InvalidOperationException($"Failed to update profile: {string.Join("; ", phoneResult.Errors.Select(e => e.Description))}");
        }
    }
}
