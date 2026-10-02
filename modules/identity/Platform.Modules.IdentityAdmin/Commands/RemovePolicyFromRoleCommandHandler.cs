namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;

/// <summary>
/// Remove policy khỏi role. Không áp C6 scope check — C6 chỉ áp dụng khi GRANT, không áp dụng
/// khi REVOKE. RoleLevel governance (C5) vẫn áp dụng.
/// </summary>
public sealed class RemovePolicyFromRoleCommandHandler : IAsyncCommandHandler<RemovePolicyFromRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public RemovePolicyFromRoleCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(RemovePolicyFromRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify policies on a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        var existingClaim = (await _roleManager.GetClaimsAsync(role))
            .FirstOrDefault(c => c.Type == "Policy" && c.Value == command.PolicyCode);

        if (existingClaim == null)
            return;

        var result = await _roleManager.RemoveClaimAsync(role, existingClaim);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to remove policy: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
