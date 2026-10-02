namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;

public sealed class AssignPolicyToRoleCommandHandler : IAsyncCommandHandler<AssignPolicyToRoleCommand>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IPolicyProvider _policyProvider;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;

    public AssignPolicyToRoleCommandHandler(
        RoleManager<Role> roleManager,
        ICurrentUser<CurrentUserInfo> currentUser,
        IPolicyProvider policyProvider,
        IRoleGovernanceValidator roleGovernanceValidator)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
    }

    public async Task HandleAsync(AssignPolicyToRoleCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var policyExists = await _policyProvider.GetPolicyByCodeAsync(command.PolicyCode, cancellationToken);
        if (policyExists == null)
            throw new InvalidOperationException($"Policy '{command.PolicyCode}' not found");

        var actorPolicies = await _policyProvider.GetPoliciesByUserAsync(actor.UserId.Value, cancellationToken);
        if (!actorPolicies.Any(p => p.Code == command.PolicyCode))
            throw new UnauthorizedAccessException($"You do not have policy '{command.PolicyCode}' to grant it");

        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify policies on a system role");

        if (!await _roleGovernanceValidator.CanManageRoleAsync(actor.UserId.Value, role.Id, cancellationToken))
            throw new UnauthorizedAccessException("You cannot modify a role with level greater than or equal to your own");

        var existingClaim = (await _roleManager.GetClaimsAsync(role))
            .FirstOrDefault(c => c.Type == "Policy" && c.Value == command.PolicyCode);

        if (existingClaim != null)
            return;

        var claim = new Claim("Policy", command.PolicyCode);
        var result = await _roleManager.AddClaimAsync(role, claim);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to assign policy: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
