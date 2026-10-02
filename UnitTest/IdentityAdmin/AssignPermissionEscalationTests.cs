namespace UnitTest.IdentityAdmin;

using System.Security.Claims;
using Platform.Modules.IdentityAdmin.Commands;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authentication.Identity.Domain;

/// <summary>
/// Grant-scope (ADR C6) with the real PermissionProvider — a mock would hide wildcard bugs.
/// </summary>
public sealed class AssignPermissionEscalationTests
{
    private static (AssignPermissionToRoleCommandHandler Handler, Mock<RoleManager<Role>> RoleManager, Role TargetRole)
        CreateHandler(GovernanceStore store, params string[] actorPermissions)
    {
        var actorId = store.AddUser(store.AddRole("OrgAdminLike", 800, permissions: actorPermissions));
        var target = store.AddRole("Lawyer", 100);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(target.Id.ToString())).ReturnsAsync(target);
        roleManager.Setup(m => m.GetClaimsAsync(target)).ReturnsAsync(new List<Claim>());
        roleManager.Setup(m => m.AddClaimAsync(target, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            store.PermissionProvider,
            store.Validator);
        return (handler, roleManager, target);
    }

    [Theory(DisplayName = "S3: actor without wildcard cannot grant a wildcard")]
    [InlineData("*")]
    [InlineData("System.All")]
    public async Task Actor_without_wildcard_cannot_grant_wildcard(string wildcard)
    {
        await using var store = new GovernanceStore();
        var (handler, roleManager, target) = CreateHandler(store, "Case.View");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = target.Id, Permission = wildcard },
            CancellationToken.None));

        roleManager.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact(DisplayName = "S3: actor cannot grant a specific permission it does not hold")]
    public async Task Actor_cannot_grant_permission_it_does_not_hold()
    {
        await using var store = new GovernanceStore();
        var (handler, _, target) = CreateHandler(store, "Case.View");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = target.Id, Permission = "Case.Approve" },
            CancellationToken.None));
    }

    [Fact(DisplayName = "S3: actor holding '*' can grant a specific permission")]
    public async Task Wildcard_actor_can_grant_specific_permission()
    {
        await using var store = new GovernanceStore();
        var (handler, roleManager, target) = CreateHandler(store, "*");

        await handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = target.Id, Permission = "Case.Approve" },
            CancellationToken.None);

        roleManager.Verify(m => m.AddClaimAsync(target, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "Case.Approve")), Times.Once);
    }
}
