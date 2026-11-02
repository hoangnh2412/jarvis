using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.IdentityAdmin.Authorization;

namespace Platform.Modules.IdentityAdmin.Services;

public sealed class IdentityInstallService(
    IPlatformIdentityStore store,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    AuthorizationDataSeeder seeder,
    SystemAuthorizationAligner aligner,
    IPasswordPolicyValidator passwordPolicy) : ISystemInstallService
{
    public const string DisplayNameClaimType = IdentityClaimTypes.DisplayName;

    public async Task<IdentityInstallOutcome> InstallAsync(IdentityInstallRequest request, CancellationToken cancellationToken = default)
    {
        var context = GetDb();
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);

        if (await context.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
            return IdentityInstallOutcome.AlreadyInstalled();

        var password = await passwordPolicy.ValidateAsync(request.Password, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (!password.Succeeded)
            return IdentityInstallOutcome.InvalidPassword(password.Errors);

        var strategy = context.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (await context.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
                    return IdentityInstallOutcome.AlreadyInstalled();

                var user = new User
                {
                    Id = Guid.CreateVersion7(),
                    UserName = request.UserName.Trim(),
                    TenantId = null,
                    OrgId = null,
                };
                var created = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);
                if (!created.Succeeded)
                {
                    return IdentityInstallOutcome.InvalidPassword(created.Errors.Select(error => error.Description).ToArray());
                }

                var adminRole = await roleManager.FindByNameAsync(SystemAuthorizationAligner.AdminRole).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("Thiếu role ADMIN sau seed.");
                var adminAssigned = await userManager.AddToRoleAsync(user, adminRole.Name!).ConfigureAwait(false);
                if (!adminAssigned.Succeeded)
                {
                    return IdentityInstallOutcome.InvalidPassword(adminAssigned.Errors.Select(error => error.Description).ToArray());
                }

                await userManager.AddClaimAsync(
                    user,
                    new Claim(PermissionCatalog.ClaimType, PlatformPermissions.TenantView)).ConfigureAwait(false);
                await userManager.AddClaimAsync(
                    user,
                    new Claim(PermissionCatalog.ClaimType, PlatformPermissions.TenantManage)).ConfigureAwait(false);
                await userManager.AddClaimAsync(
                    user,
                    new Claim(PermissionCatalog.ClaimType, PlatformPermissions.ShellAccess)).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(request.DisplayName))
                {
                    await userManager.AddClaimAsync(user, new Claim(DisplayNameClaimType, request.DisplayName.Trim()))
                        .ConfigureAwait(false);
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                await aligner.AlignAsync(cancellationToken).ConfigureAwait(false);
                return IdentityInstallOutcome.Created(user.Id);
            }).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            return IdentityInstallOutcome.AlreadyInstalled();
        }
    }

    private IdentityDbContextBase GetDb() => store.DbContext;
}
