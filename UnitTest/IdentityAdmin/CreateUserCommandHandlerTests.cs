namespace UnitTest.IdentityAdmin;

using Platform.Modules.IdentityAdmin.Commands;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authentication.Identity.Domain;

public sealed class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_creates_user_and_assigns_role()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "P@ssw0rd!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Lawyer"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddClaimAsync(It.IsAny<User>(), It.IsAny<System.Security.Claims.Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(actorId));

        var tenantId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var result = await handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "new.lawyer",
                Email = "new.lawyer@lexora.vn",
                Password = "P@ssw0rd!",
                RoleId = role.Id,
                TenantId = tenantId,
                OrgId = orgId
            },
            CancellationToken.None);

        Assert.Equal("new.lawyer", result.Username);
        // ADR Q3: TenantId / OrgId are columns on AspNetUsers.
        userManager.Verify(m => m.CreateAsync(It.Is<User>(u => u.TenantId == tenantId && u.OrgId == orgId), "P@ssw0rd!"), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "Lawyer"), Times.Once);
        // S7: no more GroupSid / OrgId user claims — the columns are the single source.
        userManager.Verify(m => m.AddClaimAsync(It.IsAny<User>(), It.IsAny<System.Security.Claims.Claim>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_actor_missing()
    {
        await using var store = new GovernanceStore();
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "x",
                Email = "x@lexora.vn",
                Password = "P@ssw0rd!",
                RoleId = Guid.NewGuid()
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_when_role_not_found()
    {
        await using var store = new GovernanceStore();
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Role?)null);

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "x",
                Email = "x@lexora.vn",
                Password = "P@ssw0rd!",
                RoleId = Guid.NewGuid()
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_when_create_fails()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);

        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(actorId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "x",
                Email = "x@lexora.vn",
                Password = "weak",
                RoleId = role.Id
            },
            CancellationToken.None));
    }

    [Theory(DisplayName = "S4/T7: cannot create a user with a role at or above own level")]
    [InlineData(800)]
    [InlineData(1000)]
    public async Task HandleAsync_throws_unauthorized_when_role_not_lower_than_actor(int targetLevel)
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800, isSystem: true));
        var role = store.AddRole("Target", targetLevel, isSystem: true);

        var userManager = IdentityMockHelpers.MockUserManager<User>();
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(actorId));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "escalated",
                Email = "escalated@lexora.vn",
                Password = "P@ssw0rd!",
                RoleId = role.Id
            },
            CancellationToken.None));

        userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "S4: actor without any role cannot create users")]
    public async Task HandleAsync_throws_unauthorized_when_actor_has_no_role()
    {
        await using var store = new GovernanceStore();
        var role = store.AddRole("Employee", 100);

        var userManager = IdentityMockHelpers.MockUserManager<User>();
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new CreateUserCommandHandler(
            userManager.Object,
            roleManager.Object,
            store.Validator,
            new FakeCurrentUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new CreateUserCommand
            {
                Username = "x",
                Email = "x@lexora.vn",
                Password = "P@ssw0rd!",
                RoleId = role.Id
            },
            CancellationToken.None));
    }
}
