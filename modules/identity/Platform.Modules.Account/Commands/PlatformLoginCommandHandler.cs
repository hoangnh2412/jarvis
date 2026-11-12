using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.Account.Services;

namespace Platform.Modules.Account.Commands;

public sealed class PlatformLoginCommandHandler(
    UserManager<User> userManager,
    AccountAccessTokenIssuer tokens) : IAsyncCommandHandler<PlatformLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(PlatformLoginCommand command, CancellationToken cancellationToken)
    {
        var missingInput = string.IsNullOrWhiteSpace(command.Username)
            || string.IsNullOrWhiteSpace(command.Password);
        if (missingInput)
            throw new BusinessException(AccountErrorCode.LoginInputRequired);

        var user = await userManager.FindByNameAsync(command.Username.Trim()).ConfigureAwait(false);
        if (user == null)
            throw new BusinessException(AccountErrorCode.InvalidCredentials);

        if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
            throw new BusinessException(AccountErrorCode.AccountLocked);

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password).ConfigureAwait(false);
        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user).ConfigureAwait(false);
            if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
                throw new BusinessException(AccountErrorCode.AccountLocked);

            throw new BusinessException(AccountErrorCode.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

        var isPlatformAccount = user.TenantId is null && user.OrgId is null;
        if (!isPlatformAccount)
            throw new BusinessException(AccountErrorCode.NotPlatformAccount);

        return new LoginResponse
        {
            AccessToken = tokens.Issue(user, AccountRealms.Platform, includeTenantClaim: false),
            RefreshToken = string.Empty,
            UserId = user.Id,
        };
    }
}
