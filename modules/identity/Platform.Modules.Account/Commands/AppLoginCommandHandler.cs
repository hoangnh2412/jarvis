using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.Account.Abstractions;
using Platform.Modules.Account.Services;
using Platform.Orgs.Abstractions;
using Platform.Tenants.Services;

namespace Platform.Modules.Account.Commands;

public sealed class AppLoginCommandHandler(
    ITenantManager tenants,
    IOrgCatalog orgs,
    IAppIdentityStore tenantIdentity,
    IPasswordHasher<User> passwordHasher,
    AccountAccessTokenIssuer tokens) : IAsyncCommandHandler<AppLoginCommand, AppLoginResult>
{
    public async Task<AppLoginResult> HandleAsync(AppLoginCommand command, CancellationToken cancellationToken)
    {
        var missingInput = string.IsNullOrWhiteSpace(command.TenantCode)
            || string.IsNullOrWhiteSpace(command.OrgCode)
            || string.IsNullOrWhiteSpace(command.UserName)
            || string.IsNullOrWhiteSpace(command.Password);
        if (missingInput)
            throw new BusinessException(AccountErrorCode.LoginInputRequired);

        var tenant = await tenants.GetByCodeAsync(command.TenantCode.Trim(), cancellationToken).ConfigureAwait(false);
        if (tenant is null || !tenant.IsActive)
            throw new BusinessException(AccountErrorCode.TenantUnavailable);

        var org = await orgs.GetByCodeAsync(command.OrgCode.Trim(), tenant.Id, cancellationToken).ConfigureAwait(false);
        if (org is null || !org.IsActive)
            throw new BusinessException(AccountErrorCode.OrgUnavailable);

        var tenantDb = tenantIdentity.DbContext;
        var normalized = command.UserName.Trim().ToUpperInvariant();
        var user = await tenantDb.Users
            .FirstOrDefaultAsync(
                row => row.NormalizedUserName == normalized && row.TenantId == tenant.Id,
                cancellationToken)
            .ConfigureAwait(false);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessException(AccountErrorCode.InvalidCredentials);

        if (user.LockoutEnd is DateTimeOffset lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
            throw new BusinessException(AccountErrorCode.AccountLocked);

        var verified = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (verified == PasswordVerificationResult.Failed)
        {
            user.AccessFailedCount += 1;
            await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw new BusinessException(AccountErrorCode.InvalidCredentials);
        }

        if (user.OrgId is not Guid assignedOrg || assignedOrg != org.Id)
            throw new BusinessException(AccountErrorCode.InvalidCredentials);

        if (user.AccessFailedCount != 0)
        {
            user.AccessFailedCount = 0;
            await tenantDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var refreshToken = await AppRefreshTokens.IssueAsync(tenantDb, user.Id, cancellationToken).ConfigureAwait(false);
        return new AppLoginResult(
            tokens.Issue(user, AccountRealms.App, includeTenantClaim: true),
            refreshToken,
            user.Id,
            tenant.Id,
            org.Id);
    }
}
