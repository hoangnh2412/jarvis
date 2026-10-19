namespace Platform.Modules.IdentityAdmin.Commands;

using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;

/// <summary>
/// Gỡ policy khỏi role. Policy là điều kiện AND nên gỡ một policy làm rộng phạm vi.
/// Actor không có wildcard chỉ được gỡ khi vẫn giữ mọi policy còn lại của role (V8).
/// RoleLevel (C5) vẫn áp dụng. System role không sửa được.
/// </summary>
public sealed class RemovePolicyFromRoleCommandHandler : IAsyncCommandHandler<RemovePolicyFromRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;
    private readonly IPermissionProvider _permissionProvider;
    private readonly IPolicyProvider _policyProvider;

    public RemovePolicyFromRoleCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IRoleGovernanceValidator roleGovernanceValidator,
        IPermissionProvider permissionProvider,
        IPolicyProvider policyProvider)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
        _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
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

        var claims = await _roleManager.GetClaimsAsync(role);
        var existingClaim = claims.FirstOrDefault(c => c.Type == "Policy" && c.Value == command.PolicyCode);
        if (existingClaim == null)
            return;

        var actorHasWildcard = await _permissionProvider.HasPermissionAsync(actor.UserId.Value, "*", cancellationToken);
        if (!actorHasWildcard)
        {
            var remaining = claims
                .Where(c => c.Type == "Policy" && c.Value != command.PolicyCode && !string.IsNullOrEmpty(c.Value))
                .Select(c => c.Value!)
                .ToList();
            if (remaining.Count > 0)
            {
                var held = (await _policyProvider.GetPoliciesByUserAsync(actor.UserId.Value, cancellationToken))
                    .Select(p => p.Code)
                    .ToHashSet(StringComparer.Ordinal);
                if (remaining.Any(code => !held.Contains(code)))
                    throw new UnauthorizedAccessException("Removing this policy would widen the role beyond your policy scope");
            }
        }

        var result = await _roleManager.RemoveClaimAsync(role, existingClaim);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to remove policy: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
