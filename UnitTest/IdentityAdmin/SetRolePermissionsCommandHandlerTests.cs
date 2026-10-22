namespace UnitTest.IdentityAdmin;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authentication.Identity;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.Modules.IdentityAdmin.Commands;

public sealed class SetRolePermissionsCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_replaces_permission_claims()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim> { new(IdentityClaimTypes.Permission, "Case.View") });
        roleManager.Setup(m => m.RemoveClaimAsync(role, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
        roleManager.Setup(m => m.AddClaimAsync(role, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var permissions = new Mock<IPermissionProvider>();
        permissions.Setup(p => p.HasPermissionAsync(actorId, "Case.Edit", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new SetRolePermissionsCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            permissions.Object,
            store.Validator);

        await handler.HandleAsync(
            new SetRolePermissionsCommand { RoleId = role.Id, Permissions = ["Case.Edit"] },
            CancellationToken.None);

        roleManager.Verify(m => m.RemoveClaimAsync(role, It.Is<Claim>(c => c.Value == "Case.View")), Times.Once);
        roleManager.Verify(m => m.AddClaimAsync(role, It.Is<Claim>(c => c.Type == IdentityClaimTypes.Permission && c.Value == "Case.Edit")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_rejects_system_role()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("Admin", 1000));
        var role = store.AddRole("ADMIN", 1000, isSystem: true);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new SetRolePermissionsCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            new Mock<IPermissionProvider>().Object,
            store.Validator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new SetRolePermissionsCommand { RoleId = role.Id, Permissions = ["Case.View"] },
            CancellationToken.None));
    }
}
