namespace UnitTest.IdentityAdmin;

using System.Security.Claims;
using Platform.Modules.IdentityAdmin.Commands;
using Microsoft.AspNetCore.Identity;
using Moq;
using Platform.Authorization.Abstractions;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;

public sealed class AssignPolicyToRoleCommandHandlerTests
{
    private static Policy CreatePolicy(string code) => new() { Id = Guid.NewGuid(), Code = code, Name = code };

    private static Mock<IPolicyProvider> ActorHoldsPolicy(Guid actorId, Policy policy)
    {
        var policyProvider = new Mock<IPolicyProvider>();
        policyProvider.Setup(p => p.GetPolicyByCodeAsync(policy.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyProvider.Setup(p => p.GetPoliciesByUserAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([policy]);
        return policyProvider;
    }

    [Fact]
    public async Task HandleAsync_adds_policy_claim_when_actor_has_scope()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var role = store.AddRole("Lawyer", 100);
        var policy = CreatePolicy("P_SAME_ORG");

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.GetClaimsAsync(role)).ReturnsAsync(new List<Claim>());
        roleManager.Setup(m => m.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Policy" && c.Value == "P_SAME_ORG")))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new AssignPolicyToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHoldsPolicy(actorId, policy).Object,
            store.Validator);

        await handler.HandleAsync(
            new AssignPolicyToRoleCommand { RoleId = role.Id, PolicyCode = "P_SAME_ORG" },
            CancellationToken.None);

        roleManager.Verify(m => m.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Policy" && c.Value == "P_SAME_ORG")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_throws_when_policy_code_not_found()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();

        var policyProvider = new Mock<IPolicyProvider>();
        policyProvider.Setup(p => p.GetPolicyByCodeAsync("P_UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var handler = new AssignPolicyToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            policyProvider.Object,
            store.Validator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new AssignPolicyToRoleCommand { RoleId = Guid.NewGuid(), PolicyCode = "P_UNKNOWN" },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_actor_lacks_policy_scope()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var policy = CreatePolicy("P_SAME_ORG");
        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();

        var policyProvider = new Mock<IPolicyProvider>();
        policyProvider.Setup(p => p.GetPolicyByCodeAsync("P_SAME_ORG", It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyProvider.Setup(p => p.GetPoliciesByUserAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new AssignPolicyToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            policyProvider.Object,
            store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPolicyToRoleCommand { RoleId = Guid.NewGuid(), PolicyCode = "P_SAME_ORG" },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_when_role_not_found()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("OrgAdmin", 800));
        var policy = CreatePolicy("P_SAME_ORG");

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Role?)null);

        var handler = new AssignPolicyToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHoldsPolicy(actorId, policy).Object,
            store.Validator);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new AssignPolicyToRoleCommand { RoleId = Guid.NewGuid(), PolicyCode = "P_SAME_ORG" },
            CancellationToken.None));
    }

    [Fact(DisplayName = "S5/T7: cannot attach a policy to a role at or above own level")]
    public async Task HandleAsync_throws_unauthorized_when_target_role_not_lower()
    {
        await using var store = new GovernanceStore();
        var actorId = store.AddUser(store.AddRole("Director", 700));
        var role = store.AddRole("OrgAdminLike", 800);
        var policy = CreatePolicy("P_SAME_ORG");

        var roleManager = IdentityMockHelpers.MockRoleManager<Role>();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new AssignPolicyToRoleCommandHandler(
            roleManager.Object,
            new FakeCurrentUser(actorId),
            ActorHoldsPolicy(actorId, policy).Object,
            store.Validator);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new AssignPolicyToRoleCommand { RoleId = role.Id, PolicyCode = "P_SAME_ORG" },
            CancellationToken.None));

        roleManager.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }
}
