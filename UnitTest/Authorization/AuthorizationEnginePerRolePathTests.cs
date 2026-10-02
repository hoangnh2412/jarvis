using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;
using Platform.Authorization.EntityFramework.Services;
using Platform.Authorization.Operands;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// End-to-end engine evaluation with real providers/evaluator and seeded policies (ADR Phụ lục D, §7 Q8).
/// Policies AND within a role; roles granting the permission OR'ed.
/// </summary>
public sealed class AuthorizationEnginePerRolePathTests
{
    private static readonly Guid OrgA = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid OrgB = Guid.Parse("00000000-0000-0000-0000-0000000000b2");
    private static readonly Guid DeptAcc = Guid.Parse("00000000-0000-0000-0000-0000000000ac");
    private static readonly Guid DeptTech = Guid.Parse("00000000-0000-0000-0000-00000000007e");

    private static readonly Guid Tenant1 = Guid.Parse("00000000-0000-0000-0000-00000000071a");
    private static readonly Guid Tenant2 = Guid.Parse("00000000-0000-0000-0000-00000000072b");

    private sealed record Expense(Guid OrgId, Guid DepartmentId, decimal Amount);

    private sealed record Customer(Guid? TenantId, Guid? OrgId, Guid DepartmentId, Guid CreatedBy);

    private static async Task<(InMemoryTestDbContext Ctx, IAuthorizationEngine Engine)> CreateAsync(OperandRegistry? operands = null)
    {
        var ctx = new InMemoryTestDbContext(new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        await ctx.SeedWithBusinessAsync();

        var engine = new AuthorizationEngine(
            new PermissionProvider(ctx),
            new PolicyProvider(ctx),
            new PolicyEvaluator(new OperandResolver(ctx, operands ?? TestOperands.Registry())));
        return (ctx, engine);
    }

    private static Guid AddRole(InMemoryTestDbContext ctx, string[] permissions, string[] policyCodes)
    {
        var roleId = Guid.NewGuid();
        foreach (var permission in permissions)
            ctx.RoleClaims.Add(new IdentityRoleClaim<Guid> { RoleId = roleId, ClaimType = "Permission", ClaimValue = permission });
        foreach (var code in policyCodes)
            ctx.RoleClaims.Add(new IdentityRoleClaim<Guid> { RoleId = roleId, ClaimType = "Policy", ClaimValue = code });
        return roleId;
    }

    private static async Task<Guid> AddUserAsync(
        InMemoryTestDbContext ctx,
        Guid[] roleIds,
        Guid? orgId,
        Guid departmentId,
        decimal? approveLimit = null,
        Guid? tenantId = null)
    {
        var userId = Guid.NewGuid();
        foreach (var roleId in roleIds)
            ctx.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });

        // ADR Q3: TenantId / OrgId are columns; DepartmentId / ApproveLimit are claims.
        ctx.Users.Add(new User { Id = userId, UserName = $"u-{userId:N}", TenantId = tenantId ?? Tenant1, OrgId = orgId });

        var claims = ctx.UserClaims;
        claims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "DepartmentId", ClaimValue = departmentId.ToString() });
        if (approveLimit.HasValue)
            claims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "ApproveLimit", ClaimValue = approveLimit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) });

        await ctx.SaveChangesAsync();
        return userId;
    }

    private static async Task<Guid> AddCfoAsync(InMemoryTestDbContext ctx)
    {
        var cfo = AddRole(ctx, ["Expense.View", "Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMIT"]);
        return await AddUserAsync(ctx, [cfo], OrgA, DeptAcc, approveLimit: 10_000_000m);
    }

    [Fact(DisplayName = "T77 / EXP-001: CFO same Org, 8M ≤ 10M → ALLOW")]
    public async Task Cfo_SameOrg_WithinLimit_Allows()
    {
        var (ctx, engine) = await CreateAsync();
        var userId = await AddCfoAsync(ctx);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 8_000_000m));

        Assert.True(result.IsAllowed);
    }

    [Fact(DisplayName = "S1 / T69: CFO same Org, 15M > 10M → DENY (SameOrg alone must not allow)")]
    public async Task Cfo_SameOrg_OverLimit_Denies()
    {
        var (ctx, engine) = await CreateAsync();
        var userId = await AddCfoAsync(ctx);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 15_000_000m));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "S1 / T78 / EXP-003: CFO other Org, 1M within limit → DENY (ApproveLimit alone must not allow)")]
    public async Task Cfo_OtherOrg_WithinLimit_Denies()
    {
        var (ctx, engine) = await CreateAsync();
        var userId = await AddCfoAsync(ctx);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgB, DeptAcc, 1_000_000m));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "T79: DeptMgr SameOrg + SameDept + 1.5M ≤ 2M → ALLOW")]
    public async Task DeptMgr_AllThreePoliciesPass_Allows()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_SAME_DEPARTMENT", "P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc, approveLimit: 2_000_000m);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 1_500_000m));

        Assert.True(result.IsAllowed);
    }

    [Fact(DisplayName = "S1 / T80: DeptMgr other Dept, 1M → DENY")]
    public async Task DeptMgr_OtherDepartment_Denies()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_SAME_DEPARTMENT", "P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc, approveLimit: 2_000_000m);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptTech, 1_000_000m));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "Q8: multi-role — one role path passing is enough (union across roles)")]
    public async Task MultiRole_AnyRolePathPasses_Allows()
    {
        var (ctx, engine) = await CreateAsync();
        var role1 = AddRole(ctx, ["Expense.View"], ["P_SAME_ORG"]);
        var role2 = AddRole(ctx, ["Expense.View"], ["P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(ctx, [role1, role2], OrgA, DeptAcc, approveLimit: 2_000_000m);

        var viaRole2 = await engine.EvaluatePoliciesAsync(userId, "Expense.View", new Expense(OrgB, DeptAcc, 1_000_000m));
        var neither = await engine.EvaluatePoliciesAsync(userId, "Expense.View", new Expense(OrgB, DeptAcc, 5_000_000m));

        Assert.True(viaRole2.IsAllowed);
        Assert.False(neither.IsAllowed);
    }

    [Fact(DisplayName = "Q8: a role that does not grant the permission contributes no path")]
    public async Task RoleWithoutPermission_PoliciesIgnored()
    {
        var (ctx, engine) = await CreateAsync();
        var approver = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMIT"]);
        var projectViewer = AddRole(ctx, ["Project.View"], ["P_SAME_DEPARTMENT"]);
        var userId = await AddUserAsync(ctx, [approver, projectViewer], OrgA, DeptAcc, approveLimit: 10_000_000m);

        // SameDepartment passes, but it belongs to a role that cannot approve expenses.
        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgB, DeptAcc, 1_000_000m));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "S2 / Q7: wildcard held via real provider → specific permission allowed, ABAC bypassed")]
    public async Task HeldWildcard_AllowsSpecificPermission()
    {
        var (ctx, engine) = await CreateAsync();
        var admin = AddRole(ctx, ["*"], []);
        var userId = await AddUserAsync(ctx, [admin], OrgA, DeptAcc);

        Assert.True(await engine.AuthorizeAsync(userId, "Expense.Approve", new Expense(OrgB, DeptTech, 99_000_000m)));
    }

    [Fact(DisplayName = "S2: user without wildcard requesting '*' → DENY")]
    public async Task RequestedWildcard_WithoutGrant_Denies()
    {
        var (ctx, engine) = await CreateAsync();
        var userId = await AddCfoAsync(ctx);

        Assert.False(await engine.HasPermissionAsync(userId, "*"));
        Assert.False(await engine.AuthorizeAsync(userId, "System.All", new Expense(OrgA, DeptAcc, 1m)));
    }

    [Fact(DisplayName = "Unknown policy code: provider throws, listing the missing code")]
    public async Task PolicyProvider_UnknownCode_Throws()
    {
        var (ctx, _) = await CreateAsync();
        var role = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMT"]);
        await ctx.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnresolvedPolicyException>(
            () => new PolicyProvider(ctx).GetPoliciesByRoleAsync(role));

        Assert.Equal(role, ex.RoleId);
        Assert.Equal(new[] { "P_APPROVE_LIMT" }, ex.MissingCodes);
    }

    [Fact(DisplayName = "Unknown policy code: path fails closed (typo'd ApproveLimit must not leave SameOrg alone)")]
    public async Task UnknownPolicyCode_PathFailsClosed()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMT"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc, approveLimit: 10_000_000m);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 15_000_000m));

        Assert.False(result.IsAllowed);
        Assert.Contains("P_APPROVE_LIMT", result.Reason);
    }

    [Fact(DisplayName = "Unknown policy code on one role does not block a valid role path")]
    public async Task UnknownPolicyCode_OtherValidRoleStillAllows()
    {
        var (ctx, engine) = await CreateAsync();
        var broken = AddRole(ctx, ["Expense.Approve"], ["P_DOES_NOT_EXIST"]);
        var valid = AddRole(ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(ctx, [broken, valid], OrgA, DeptAcc, approveLimit: 10_000_000m);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 8_000_000m));

        Assert.True(result.IsAllowed);
    }

    [Fact(DisplayName = "S7 / T60–T61: P_SAME_TENANT reads TenantId from the AspNetUsers column")]
    public async Task SameTenant_UsesTenantColumn()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Company.Config.Edit"], ["P_SAME_TENANT"]);
        var userId = await AddUserAsync(ctx, [role], orgId: null, DeptTech, tenantId: Tenant1);

        var same = await engine.EvaluatePoliciesAsync(userId, "Company.Config.Edit", new Customer(Tenant1, OrgA, DeptAcc, Guid.NewGuid()));
        var other = await engine.EvaluatePoliciesAsync(userId, "Company.Config.Edit", new Customer(Tenant2, OrgA, DeptAcc, Guid.NewGuid()));

        Assert.True(same.IsAllowed);
        Assert.False(other.IsAllowed);
    }

    [Fact(DisplayName = "S7 / T66–T67: P_OWNER_ONLY resolves User.Id to the subject itself")]
    public async Task OwnerOnly_UsesSubjectId()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["SalesResult.View"], ["P_OWNER_ONLY"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc);

        var mine = await engine.EvaluatePoliciesAsync(userId, "SalesResult.View", new Customer(Tenant1, OrgA, DeptAcc, userId));
        var others = await engine.EvaluatePoliciesAsync(userId, "SalesResult.View", new Customer(Tenant1, OrgA, DeptAcc, Guid.NewGuid()));

        Assert.True(mine.IsAllowed);
        Assert.False(others.IsAllowed);
    }

    [Fact(DisplayName = "S7 / T74–T76: P_OWNER_OR_SAME_DEPT — owner OR same department")]
    public async Task OwnerOrSameDept_RealStore()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Customer.View"], ["P_OWNER_OR_SAME_DEPT"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptTech);

        var ownerOtherDept = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, OrgA, DeptAcc, userId));
        var sameDeptNotOwner = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, OrgA, DeptTech, Guid.NewGuid()));
        var neither = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, OrgA, DeptAcc, Guid.NewGuid()));

        Assert.True(ownerOtherDept.IsAllowed);
        Assert.True(sameDeptNotOwner.IsAllowed);
        Assert.False(neither.IsAllowed);
    }

    [Fact(DisplayName = "S7: a stale OrgId user claim is ignored — the column is authoritative")]
    public async Task OrgIdClaim_IsIgnored()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Customer.View"], ["P_SAME_ORG"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc);
        ctx.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "OrgId", ClaimValue = OrgB.ToString() });
        await ctx.SaveChangesAsync();

        var orgB = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, OrgB, DeptAcc, Guid.NewGuid()));
        var orgA = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, OrgA, DeptAcc, Guid.NewGuid()));

        Assert.False(orgB.IsAllowed);
        Assert.True(orgA.IsAllowed);
    }

    [Fact(DisplayName = "Missing attribute never matches: user without OrgId vs resource without OrgId → DENY")]
    public async Task NullAttributes_DoNotMatch()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Customer.View"], ["P_SAME_ORG"]);
        var userId = await AddUserAsync(ctx, [role], orgId: null, DeptAcc);

        var result = await engine.EvaluatePoliciesAsync(userId, "Customer.View", new Customer(Tenant1, null, DeptAcc, Guid.NewGuid()));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "Registry: business policy fails closed when the host did not register its operands")]
    public async Task BusinessPolicy_FailsClosedWithoutOperandRegistration()
    {
        var (ctx, engine) = await CreateAsync(OperandRegistry.Build());
        var userId = await AddCfoAsync(ctx);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 1m));

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "Registry: collection operand makes the seeded P_MANAGED_PROJECT (IN) evaluable")]
    public async Task ManagedProject_InCollection()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Project.Edit"], ["P_MANAGED_PROJECT"]);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptTech);
        ctx.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "ManagedDepartmentIds", ClaimValue = $"{DeptTech},{DeptAcc}" });
        await ctx.SaveChangesAsync();

        var managed = await engine.EvaluatePoliciesAsync(userId, "Project.Edit", new Customer(Tenant1, OrgA, DeptAcc, Guid.NewGuid()));
        var other = await engine.EvaluatePoliciesAsync(userId, "Project.Edit", new Customer(Tenant1, OrgA, Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(managed.IsAllowed);
        Assert.False(other.IsAllowed);
    }

    [Fact(DisplayName = "Role grants permission but has no policy → DENY (deny-by-default)")]
    public async Task GrantingRoleWithoutPolicies_Denies()
    {
        var (ctx, engine) = await CreateAsync();
        var role = AddRole(ctx, ["Expense.Approve"], []);
        var userId = await AddUserAsync(ctx, [role], OrgA, DeptAcc);

        var result = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", new Expense(OrgA, DeptAcc, 1m));

        Assert.False(result.IsAllowed);
        Assert.False(string.IsNullOrWhiteSpace(result.Reason));
    }
}
