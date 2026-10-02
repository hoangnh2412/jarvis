namespace UnitTest.IdentityAdmin;

using System.Security.Claims;
using Platform.Modules.IdentityAdmin.Commands;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authorization.Abstractions;
using Platform.Authentication.Identity.Domain;

public sealed class AssignPermissionToRoleCommandHandlerTests
{
    private static Mock<IPermissionProvider> ActorHolds(Guid actorId, string permission)
    {
        var permissionProvider = new Mock<IPermissionProvider>();
        permissionProvider.Setup(p => p.HasPermissionAsync(actorId, permission, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return permissionProvider;
    }

    [Fact]
    public async Task HandleAsync_adds_permission_claim_when_actor_has_scope()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.GetClaimsAsync(role)).ReturnsAsync(new List<Claim>());
        roleManager.Setup(m => m.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "Case.View")))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHolds(actorId, "Case.View").Object,
            store.Validator);

        await handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = role.Id, Permission = "Case.View" },
            CancellationToken.None);

        roleManager.Verify(m => m.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "Case.View")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_is_idempotent_when_claim_already_exists()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim> { new("Permission", "Case.View") });

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHolds(actorId, "Case.View").Object,
            store.Validator);

        await handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = role.Id, Permission = "Case.View" },
            CancellationToken.None);

        roleManager.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_actor_lacks_permission_scope()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();

        var permissionProvider = new Mock<IPermissionProvider>();
        permissionProvider.Setup(p => p.HasPermissionAsync(actorId, "Case.Delete", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            permissionProvider.Object,
            store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = Guid.NewGuid(), Permission = "Case.Delete" },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_when_role_not_found()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Role?)null);

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHolds(actorId, "Case.View").Object,
            store.Validator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = Guid.NewGuid(), Permission = "Case.View" },
            CancellationToken.None));
    }

    [Theory(DisplayName = "S5/T7: cannot modify a role at or above own level, even holding the permission")]
    [InlineData(650)]
    [InlineData(500)]
    public async Task HandleAsync_throws_unauthorized_when_target_role_not_lower(int targetLevel)
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("DeptMgr", 500));
        var role = store.AddRole("Cfo", targetLevel);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new AssignPermissionToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHolds(actorId, "Expense.Approve").Object,
            store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPermissionToRoleCommand { RoleId = role.Id, Permission = "Expense.Approve" },
            CancellationToken.None));

        roleManager.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }
}
