using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.DDD.Domain.Shared.Extensions;
using Platform.Modules.Account.Abstractions;
using Platform.Modules.Account.Services;

namespace Platform.Modules.Account.Commands;

public sealed class AppRefreshTokenCommandHandler(
    IAppIdentityStore tenantIdentity,
    AccountAccessTokenIssuer tokens) : IAsyncCommandHandler<AppRefreshTokenCommand, AppLoginResult>
{
    public async Task<AppLoginResult> HandleAsync(AppRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            throw new BusinessException(AccountErrorCode.RefreshTokenInvalid);

        var db = tenantIdentity.DbContext;
        var hash = AppRefreshTokens.Hash(command.RefreshToken.Trim());
        var row = await db.Set<UserRefreshToken>()
            .FirstOrDefaultAsync(item => item.TokenHash == hash, cancellationToken)
            .ConfigureAwait(false);
        if (row is null || row.RevokedAtUtc is not null || row.ExpiresAtUtc <= DateTime.UtcNow)
            throw new BusinessException(AccountErrorCode.RefreshTokenInvalid);

        var user = await db.Users
            .FirstOrDefaultAsync(item => item.Id == row.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null || user.TenantId is not Guid tenantId || user.OrgId is not Guid orgId)
            throw new BusinessException(AccountErrorCode.RefreshTokenInvalid);

        row.RevokedAtUtc = DateTimeExtension.UtcTimestamp();
        var nextRefreshToken = await AppRefreshTokens.IssueAsync(db, user.Id, cancellationToken).ConfigureAwait(false);
        return new AppLoginResult(
            tokens.Issue(user, AccountRealms.App, includeTenantClaim: true),
            nextRefreshToken,
            user.Id,
            tenantId,
            orgId);
    }
}
