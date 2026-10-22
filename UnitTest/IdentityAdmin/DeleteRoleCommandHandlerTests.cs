namespace UnitTest.IdentityAdmin;

using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authentication.Identity.Domain;
using Platform.Modules.IdentityAdmin.Commands;

public sealed class DeleteRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_rejects_system_role()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("Admin", 1000));
        var role = store.AddRole("ADMIN", 900, isSystem: true);

        var handler = new DeleteRoleCommandHandler(
            IdentityMockHelpers.MockUserManager<User>().Object,
            RoleManagerReturning(role),
            new FakeCurrentUser(actorId),
            store.Validator);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new DeleteRoleCommand { RoleId = role.Id },
            CancellationToken.None));

        Assert.Contains("system role", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_rejects_role_still_assigned()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var users = IdentityMockHelpers.MockUserManager<User>();
        users.Setup(m => m.GetUsersInRoleAsync("Lawyer"))
            .ReturnsAsync(new List<User> { new() { Id = Guid.NewGuid(), UserName = "kept" } });

        var handler = new DeleteRoleCommandHandler(
            users.Object,
            RoleManagerReturning(role),
            new FakeCurrentUser(actorId),
            store.Validator);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new DeleteRoleCommand { RoleId = role.Id },
            CancellationToken.None));

        Assert.Contains("still assigned", error.Message, StringComparison.Ordinal);
    }

    private static RoleManager<Role> RoleManagerReturning(Role role)
    {
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        return roleManager.Object;
    }
}
