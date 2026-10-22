namespace Platform.Modules.IdentityAdmin.Commands;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Services;
using Platform.Orgs.Abstractions;

public sealed class CreateUserCommandHandler : IAsyncCommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRoleGovernanceValidator _roleGovernanceValidator;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IOrgCatalog? _orgCatalog;

    public CreateUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IRoleGovernanceValidator roleGovernanceValidator,
        ICurrentUser<CurrentUserInfo> currentUser,
        IOrgCatalog? orgCatalog = null)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _roleGovernanceValidator = roleGovernanceValidator ?? throw new ArgumentNullException(nameof(roleGovernanceValidator));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _orgCatalog = orgCatalog;
    }

    public async Task<CreateUserResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var targetRoles = await LoadAssignableRolesAsync(actor.UserId.Value, ResolveRoleIds(command), cancellationToken);

        var tenantId = command.TenantId;
        var orgId = command.OrgId;
        if (tenantId is null && orgId is null)
        {
            var actorUser = await _userManager.FindByIdAsync(actor.UserId.Value.ToString());
            if (actorUser != null)
            {
                tenantId = actorUser.TenantId;
                orgId = actorUser.OrgId;
            }
        }

        if (!await _roleGovernanceValidator.IsWithinActorScopeAsync(actor.UserId.Value, tenantId, orgId, cancellationToken))
            throw new UnauthorizedAccessException("You cannot create a user outside your tenant or organization");

        if (orgId is Guid assignedOrgId)
        {
            if (_orgCatalog is null)
                throw new InvalidOperationException("IOrgCatalog is not registered; cannot assign OrgId.");

            if (tenantId is not Guid assignedTenantId)
                throw new InvalidOperationException("TenantId is required when OrgId is set.");

            await _orgCatalog.ValidateForAssignmentAsync(assignedOrgId, assignedTenantId, cancellationToken)
                .ConfigureAwait(false);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = command.Username,
            Email = command.Email,
            PhoneNumber = NormalizeOptional(command.PhoneNumber),
            NormalizedUserName = command.Username.ToUpperInvariant(),
            NormalizedEmail = command.Email.ToUpperInvariant(),
            TenantId = tenantId,
            OrgId = orgId
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");

        foreach (var targetRole in targetRoles)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, targetRole.Name ?? string.Empty);
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException($"Failed to assign role: {string.Join("; ", addRoleResult.Errors.Select(e => e.Description))}");
            }
        }

        var displayName = NormalizeOptional(command.DisplayName);
        if (displayName != null)
        {
            var claimResult = await _userManager.AddClaimAsync(user, new Claim(IdentityClaimTypes.DisplayName, displayName));
            if (!claimResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException($"Failed to store display name: {string.Join("; ", claimResult.Errors.Select(e => e.Description))}");
            }
        }

        return new CreateUserResponse
        {
            UserId = user.Id,
            Username = user.UserName!
        };
    }

    internal static List<Guid> ResolveRoleIds(CreateUserCommand command)
    {
        var roleIds = new List<Guid>();
        if (command.RoleId != Guid.Empty)
            roleIds.Add(command.RoleId);

        if (command.RoleIds != null)
        {
            foreach (var roleId in command.RoleIds)
            {
                if (roleId != Guid.Empty && !roleIds.Contains(roleId))
                    roleIds.Add(roleId);
            }
        }

        if (roleIds.Count == 0)
            throw new InvalidOperationException("At least one role is required");

        return roleIds;
    }

    private async Task<List<Role>> LoadAssignableRolesAsync(Guid actorUserId, IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken)
    {
        var roles = new List<Role>(roleIds.Count);
        foreach (var roleId in roleIds)
        {
            var targetRole = await _roleManager.FindByIdAsync(roleId.ToString());
            if (targetRole == null)
                throw new InvalidOperationException($"Role {roleId} not found");

            var canAssign = await _roleGovernanceValidator.CanAssignRoleAsync(actorUserId, roleId, cancellationToken);
            if (!canAssign)
                throw new UnauthorizedAccessException("You do not have permission to assign this role");

            roles.Add(targetRole);
        }

        return roles;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
