namespace UnitTest.IdentityAdmin;

using System.Security.Claims;
using Platform.Modules.IdentityAdmin.Commands;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;

/// <summary>
/// S5: RoleLevel governance (ADR C5) on revoke, lock/unlock, delete and create-role.
/// </summary>
public sealed class GovernanceEnforcementTests
{
    private static Mock<RoleManager<Role>> RoleManagerFor(Role role, params Claim[] claims)
    {
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.GetClaimsAsync(role)).ReturnsAsync(claims.ToList());
        roleManager.Setup(m => m.RemoveClaimAsync(role, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
        roleManager.Setup(m => m.CreateAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Success);
        return roleManager;
    }

    private static Mock<UserManager<User>> UserManagerFor(Guid userId)
    {
        var user = new User { Id = userId, UserName = "target" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.SetLockoutEnabledAsync(user, It.IsAny<bool>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
        return userManager;
    }

    // --- Remove permission / policy ---

    [Fact(DisplayName = "S5: lower-level actor cannot strip a permission from a higher role")]
    public async Task RemovePermission_from_higher_role_denied()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("DeptMgr", 500));
        var cfo = store.AddRole("Cfo", 650);
        var roleManager = RoleManagerFor(cfo, new Claim("Permission", "Expense.Approve"));

        var handler = new RemovePermissionFromRoleCommandHandler(roleManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new RemovePermissionFromRoleCommand { RoleId = cfo.Id, Permission = "Expense.Approve" },
            CancellationToken.None));
        roleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact(DisplayName = "S5: higher-level actor can strip a permission from a lower role")]
    public async Task RemovePermission_from_lower_role_allowed()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var cfo = store.AddRole("Cfo", 650);
        var roleManager = RoleManagerFor(cfo, new Claim("Permission", "Expense.Approve"));

        var handler = new RemovePermissionFromRoleCommandHandler(roleManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await handler.HandleAsync(
            new RemovePermissionFromRoleCommand { RoleId = cfo.Id, Permission = "Expense.Approve" },
            CancellationToken.None);
        roleManager.Verify(m => m.RemoveClaimAsync(cfo, It.Is<Claim>(c => c.Value == "Expense.Approve")), Times.Once);
    }

    [Fact(DisplayName = "S5: equal-level actor cannot strip a policy")]
    public async Task RemovePolicy_from_equal_role_denied()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("Cfo", 650));
        var cto = store.AddRole("Cto", 650);
        var roleManager = RoleManagerFor(cto, new Claim("Policy", "P_APPROVE_LIMIT"));

        var handler = new RemovePolicyFromRoleCommandHandler(roleManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new RemovePolicyFromRoleCommand { RoleId = cto.Id, PolicyCode = "P_APPROVE_LIMIT" },
            CancellationToken.None));
        roleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact(DisplayName = "S5: higher-level actor can strip a policy from a lower role")]
    public async Task RemovePolicy_from_lower_role_allowed()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var cfo = store.AddRole("Cfo", 650);
        var roleManager = RoleManagerFor(cfo, new Claim("Policy", "P_APPROVE_LIMIT"));

        var handler = new RemovePolicyFromRoleCommandHandler(roleManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await handler.HandleAsync(
            new RemovePolicyFromRoleCommand { RoleId = cfo.Id, PolicyCode = "P_APPROVE_LIMIT" },
            CancellationToken.None);
        roleManager.Verify(m => m.RemoveClaimAsync(cfo, It.Is<Claim>(c => c.Value == "P_APPROVE_LIMIT")), Times.Once);
    }

    // --- Lock / unlock / delete user ---

    [Theory(DisplayName = "S5: OrgAdmin cannot lock/unlock an Admin")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LockUnlock_higher_user_denied(bool isLocked)
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800, isSystem: true));
        var adminUserId = store.AddUser(store.AddRole("Admin", 1000, isSystem: true));
        var userManager = UserManagerFor(adminUserId);

        var handler = new UpdateUserCommandHandler(userManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new UpdateUserCommand { UserId = adminUserId, IsLocked = isLocked },
            CancellationToken.None));
        userManager.Verify(m => m.SetLockoutEndDateAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset?>()), Times.Never);
    }

    [Fact(DisplayName = "S5: actor cannot lock a peer at the same level")]
    public async Task Lock_peer_denied()
    {
        await using var store = new GovernanceStore();
        var orgAdmin = store.AddRole("OrgAdmin", 800, isSystem: true);
        var actorId = store.AddUser(orgAdmin);
        var peerId = store.AddUser(orgAdmin);
        var userManager = UserManagerFor(peerId);

        var handler = new UpdateUserCommandHandler(userManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new UpdateUserCommand { UserId = peerId, IsLocked = true },
            CancellationToken.None));
    }

    [Fact(DisplayName = "S5: OrgAdmin can lock a lower-level user")]
    public async Task Lock_lower_user_allowed()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800, isSystem: true));
        var employeeId = store.AddUser(store.AddRole("Employee", 100));
        var userManager = UserManagerFor(employeeId);

        var handler = new UpdateUserCommandHandler(userManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await handler.HandleAsync(new UpdateUserCommand { UserId = employeeId, IsLocked = true }, CancellationToken.None);

        userManager.Verify(m => m.SetLockoutEndDateAsync(It.Is<User>(u => u.Id == employeeId), DateTimeOffset.MaxValue), Times.Once);
    }

    [Fact(DisplayName = "S5: OrgAdmin cannot delete a TenantAdmin")]
    public async Task Delete_higher_user_denied()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800, isSystem: true));
        var tenantAdminId = store.AddUser(store.AddRole("TenantAdmin", 900, isSystem: true));
        var userManager = UserManagerFor(tenantAdminId);

        var handler = new DeleteUserCommandHandler(userManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new DeleteUserCommand { UserId = tenantAdminId },
            CancellationToken.None));
        userManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact(DisplayName = "S5: Admin can delete a lower-level user")]
    public async Task Delete_lower_user_allowed()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("Admin", 1000, isSystem: true));
        var cfoId = store.AddUser(store.AddRole("Cfo", 650));
        var userManager = UserManagerFor(cfoId);

        var handler = new DeleteUserCommandHandler(userManager.Object, new FakeCurrentUser(actorId), store.Validator);

        await handler.HandleAsync(new DeleteUserCommand { UserId = cfoId }, CancellationToken.None);

        userManager.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == cfoId)), Times.Once);
    }

    // --- Create role ---

    [Theory(DisplayName = "Phụ lục B: OrgAdmin creates role level 500 → ALLOW; 800/900 → DENY")]
    [InlineData(500, true)]
    [InlineData(800, false)]
    [InlineData(900, false)]
    public async Task CreateRole_level_governed(int level, bool allowed)
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800, isSystem: true));
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.CreateAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Success);

        var handler = new CreateRoleCommandHandler(roleManager.Object, new FakeCurrentUser(actorId), store.Validator);
        var command = new CreateRoleCommand { Name = $"ROLE_{level}", RoleLevel = level };

        if (allowed)
        {
            var result = await handler.HandleAsync(command, CancellationToken.None);
            Assert.Equal($"ROLE_{level}", result.Name);
            roleManager.Verify(m => m.CreateAsync(It.Is<Role>(r => r.RoleLevel == level && !r.IsSystemRole)), Times.Once);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(command, CancellationToken.None));
            roleManager.Verify(m => m.CreateAsync(It.IsAny<Role>()), Times.Never);
        }
    }
}
