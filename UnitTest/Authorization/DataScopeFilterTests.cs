using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Services;
using Platform.Authorization.Operands;
using Platform.Authorization.Services;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// Data Scope query filter (ADR §5.11, T10): policies become a Where expression the provider translates;
/// semantics match the engine (per-role path AND, roles OR, rules OR, wildcard bypass, fail-closed).
/// </summary>
public sealed class DataScopeFilterTests
{
    private static readonly Guid Tenant1 = Guid.Parse("00000000-0000-0000-0000-00000000071a");
    private static readonly Guid Tenant2 = Guid.Parse("00000000-0000-0000-0000-00000000072b");
    private static readonly Guid OrgA = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid OrgB = Guid.Parse("00000000-0000-0000-0000-0000000000b2");
    private static readonly Guid DeptAcc = Guid.Parse("00000000-0000-0000-0000-0000000000ac");
    private static readonly Guid DeptTech = Guid.Parse("00000000-0000-0000-0000-00000000007e");
    private static readonly Guid DeptSales = Guid.Parse("00000000-0000-0000-0000-0000000000a5");

    public sealed class ExpenseRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public Guid? OrgId { get; set; }
        public Guid DepartmentId { get; set; }
        public decimal Amount { get; set; }
        public Guid CreatedBy { get; set; }
    }

    /// <summary>Resource without DepartmentId / Amount: business rules on it can never pass.</summary>
    public sealed class NoteRow
    {
        public Guid Id { get; set; }
        public Guid? OrgId { get; set; }
    }

    private sealed class Fixture
    {
        public required InMemoryTestDbContext Ctx { get; init; }
        public required DataScopeFilter Filter { get; init; }
        public required IAuthorizationEngine Engine { get; init; }
    }

    private static async Task<Fixture> CreateAsync()
    {
        var ctx = new InMemoryTestDbContext(new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        await ctx.SeedWithBusinessAsync();

        var registry = TestOperands.Registry();
        var resolver = new OperandResolver(ctx, registry);
        var evaluator = new PolicyEvaluator(resolver);
        var permissions = new PermissionProvider(ctx);
        var policies = new PolicyProvider(ctx);

        return new Fixture
        {
            Ctx = ctx,
            Filter = new DataScopeFilter(permissions, policies, evaluator, resolver, registry),
            Engine = new AuthorizationEngine(permissions, policies, evaluator),
        };
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
        Guid? orgId = null,
        Guid? departmentId = null,
        decimal? approveLimit = null,
        string? managedDepartmentIds = null,
        Guid? tenantId = null)
    {
        var userId = Guid.NewGuid();
        foreach (var roleId in roleIds)
            ctx.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });

        ctx.Users.Add(new User { Id = userId, UserName = $"u-{userId:N}", TenantId = tenantId ?? Tenant1, OrgId = orgId });

        void Claim(string type, string value) =>
            ctx.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = type, ClaimValue = value });
        if (departmentId.HasValue)
            Claim("DepartmentId", departmentId.Value.ToString());
        if (approveLimit.HasValue)
            Claim("ApproveLimit", approveLimit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (managedDepartmentIds != null)
            Claim("ManagedDepartmentIds", managedDepartmentIds);

        await ctx.SaveChangesAsync();
        return userId;
    }

    private static List<ExpenseRow> Rows(Guid owner) =>
    [
        new() { Code = "A-ACC-8M", TenantId = Tenant1, OrgId = OrgA, DepartmentId = DeptAcc, Amount = 8_000_000m, CreatedBy = Guid.NewGuid() },
        new() { Code = "A-ACC-15M", TenantId = Tenant1, OrgId = OrgA, DepartmentId = DeptAcc, Amount = 15_000_000m, CreatedBy = Guid.NewGuid() },
        new() { Code = "A-TECH-1M", TenantId = Tenant1, OrgId = OrgA, DepartmentId = DeptTech, Amount = 1_000_000m, CreatedBy = Guid.NewGuid() },
        new() { Code = "A-SALES-OWN", TenantId = Tenant1, OrgId = OrgA, DepartmentId = DeptSales, Amount = 2_000_000m, CreatedBy = owner },
        new() { Code = "B-ACC-1M", TenantId = Tenant1, OrgId = OrgB, DepartmentId = DeptAcc, Amount = 1_000_000m, CreatedBy = Guid.NewGuid() },
        new() { Code = "NOORG-ACC-1M", TenantId = Tenant1, OrgId = null, DepartmentId = DeptAcc, Amount = 1_000_000m, CreatedBy = Guid.NewGuid() },
        new() { Code = "T2-ACC-1M", TenantId = Tenant2, OrgId = OrgA, DepartmentId = DeptAcc, Amount = 1_000_000m, CreatedBy = Guid.NewGuid() },
    ];

    private static async Task<string[]> ScopeAsync(Fixture f, Guid userId, string permission, IEnumerable<ExpenseRow> rows)
    {
        var filtered = await f.Filter.ApplyFilterAsync(rows.AsQueryable(), userId, permission);
        return filtered.Select(r => r.Code).OrderBy(c => c, StringComparer.Ordinal).ToArray();
    }

    [Fact(DisplayName = "T10: no granting role → empty scope (deny-by-default)")]
    public async Task NoPermission_ReturnsNothing()
    {
        var f = await CreateAsync();
        var role = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_ORG"]);
        var userId = await AddUserAsync(f.Ctx, [role], OrgA);

        Assert.Empty(await ScopeAsync(f, userId, "Expense.Approve", Rows(userId)));
    }

    [Fact(DisplayName = "T10 / Q7: wildcard permission → no filter")]
    public async Task Wildcard_ReturnsEverything()
    {
        var f = await CreateAsync();
        var role = AddRole(f.Ctx, ["*"], []);
        var userId = await AddUserAsync(f.Ctx, [role]);

        Assert.Null(await f.Filter.BuildPredicateAsync<ExpenseRow>(userId, "Expense.Delete"));
        Assert.Equal(Rows(userId).Count, (await ScopeAsync(f, userId, "Expense.Delete", Rows(userId))).Length);
    }

    [Fact(DisplayName = "T10: role grants permission without policy → empty scope")]
    public async Task GrantingRoleWithoutPolicy_ReturnsNothing()
    {
        var f = await CreateAsync();
        var role = AddRole(f.Ctx, ["Expense.View"], []);
        var userId = await AddUserAsync(f.Ctx, [role], OrgA);

        Assert.Empty(await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10 / S1: policies AND within a role — SameOrg ∧ ApproveLimit")]
    public async Task PoliciesWithinRole_AreAnded()
    {
        var f = await CreateAsync();
        var cfo = AddRole(f.Ctx, ["Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(f.Ctx, [cfo], OrgA, DeptAcc, approveLimit: 10_000_000m);

        Assert.Equal(["A-ACC-8M", "A-SALES-OWN", "A-TECH-1M", "T2-ACC-1M"], await ScopeAsync(f, userId, "Expense.Approve", Rows(userId)));
    }

    [Fact(DisplayName = "T10 / Q8: roles granting the permission OR — SameDept ∨ Owner")]
    public async Task GrantingRoles_AreOred()
    {
        var f = await CreateAsync();
        var deptViewer = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_TENANT", "P_SAME_DEPARTMENT"]);
        var owner = AddRole(f.Ctx, ["Expense.View"], ["P_OWNER_ONLY"]);
        var userId = await AddUserAsync(f.Ctx, [deptViewer, owner], OrgA, DeptTech);

        Assert.Equal(["A-SALES-OWN", "A-TECH-1M"], await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10 / Q8: policies of a role not granting the permission do not narrow the scope")]
    public async Task NonGrantingRolePolicies_AreIgnored()
    {
        var f = await CreateAsync();
        var viewer = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_DEPARTMENT"]);
        var approver = AddRole(f.Ctx, ["Expense.Approve"], ["P_APPROVE_LIMIT"]);
        var userId = await AddUserAsync(f.Ctx, [viewer, approver], OrgA, DeptAcc, approveLimit: 1m);

        Assert.Equal(["A-ACC-15M", "A-ACC-8M", "B-ACC-1M", "NOORG-ACC-1M", "T2-ACC-1M"], await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10 / Q5: rules OR within a policy — P_OWNER_OR_SAME_DEPT")]
    public async Task RulesWithinPolicy_AreOred()
    {
        var f = await CreateAsync();
        var employee = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_TENANT", "P_OWNER_OR_SAME_DEPT"]);
        var userId = await AddUserAsync(f.Ctx, [employee], OrgA, DeptTech);

        Assert.Equal(["A-SALES-OWN", "A-TECH-1M"], await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10: IN against a collection claim — Resource.DepartmentId IN User.ManagedDepartmentIds")]
    public async Task InOperator_UsesCollection()
    {
        var f = await CreateAsync();
        var manager = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_ORG", "P_MANAGED_PROJECT"]);
        var userId = await AddUserAsync(f.Ctx, [manager], OrgA, managedDepartmentIds: $"{DeptTech},{DeptSales}");

        Assert.Equal(["A-SALES-OWN", "A-TECH-1M"], await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10: unresolved policy code removes only that role path")]
    public async Task UnresolvedPolicy_FailsClosedForThatRole()
    {
        var f = await CreateAsync();
        var broken = AddRole(f.Ctx, ["Expense.View"], ["P_DOES_NOT_EXIST"]);
        var owner = AddRole(f.Ctx, ["Expense.View"], ["P_OWNER_ONLY"]);
        var brokenOnly = await AddUserAsync(f.Ctx, [broken], OrgA);
        var both = await AddUserAsync(f.Ctx, [broken, owner], OrgA);

        Assert.Empty(await ScopeAsync(f, brokenOnly, "Expense.View", Rows(brokenOnly)));
        Assert.Equal(["A-SALES-OWN"], await ScopeAsync(f, both, "Expense.View", Rows(both)));
    }

    [Fact(DisplayName = "T10: missing user attribute (no DepartmentId claim) → rule fails")]
    public async Task MissingUserAttribute_ReturnsNothing()
    {
        var f = await CreateAsync();
        var viewer = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_DEPARTMENT"]);
        var userId = await AddUserAsync(f.Ctx, [viewer], OrgA);

        Assert.Empty(await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10: null resource attribute never matches, including !=")]
    public async Task NullResourceAttribute_NeverMatches()
    {
        var f = await CreateAsync();
        var sameOrg = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_ORG"]);
        var userId = await AddUserAsync(f.Ctx, [sameOrg], OrgB);
        f.Ctx.Policies.Add(new Policy
        {
            Id = Guid.NewGuid(),
            Code = "P_OTHER_ORG",
            Name = "Other org",
            Rules = [new PolicyRule { Id = Guid.NewGuid(), Order = 1, LeftOperand = "Resource.OrgId", Operator = "!=", RightOperand = "User.OrgId" }],
        });
        var otherOrg = AddRole(f.Ctx, ["Expense.Audit"], ["P_OTHER_ORG"]);
        var auditor = await AddUserAsync(f.Ctx, [otherOrg], OrgB);

        Assert.Equal(["B-ACC-1M"], await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
        Assert.DoesNotContain("NOORG-ACC-1M", await ScopeAsync(f, auditor, "Expense.Audit", Rows(auditor)));
    }

    [Fact(DisplayName = "T10: resource type without the attribute → rule fails (no exception)")]
    public async Task ResourceWithoutAttribute_ReturnsNothing()
    {
        var f = await CreateAsync();
        var viewer = AddRole(f.Ctx, ["Note.View"], ["P_SAME_DEPARTMENT"]);
        var userId = await AddUserAsync(f.Ctx, [viewer], OrgA, DeptAcc);

        var notes = new[] { new NoteRow { Id = Guid.NewGuid(), OrgId = OrgA } }.AsQueryable();

        Assert.Empty(await f.Filter.ApplyFilterAsync(notes, userId, "Note.View"));
    }

    [Fact(DisplayName = "T10: unregistered operand in a stored rule → rule fails at query time")]
    public async Task UnregisteredOperand_ReturnsNothing()
    {
        var f = await CreateAsync();
        f.Ctx.Policies.Add(new Policy
        {
            Id = Guid.NewGuid(),
            Code = "P_BY_AMOUNT_ONLY",
            Name = "Unregistered",
            Rules = [new PolicyRule { Id = Guid.NewGuid(), Order = 1, LeftOperand = "Resource.Code", Operator = "==", RightOperand = "User.Id" }],
        });
        var role = AddRole(f.Ctx, ["Expense.View"], ["P_BY_AMOUNT_ONLY"]);
        var userId = await AddUserAsync(f.Ctx, [role], OrgA);

        Assert.Empty(await ScopeAsync(f, userId, "Expense.View", Rows(userId)));
    }

    [Fact(DisplayName = "T10: list scope equals per-record engine decision for every row")]
    public async Task Scope_MatchesEngineDecision()
    {
        var f = await CreateAsync();
        var cfo = AddRole(f.Ctx, ["Expense.View", "Expense.Approve"], ["P_SAME_ORG", "P_APPROVE_LIMIT"]);
        var employee = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_TENANT", "P_OWNER_OR_SAME_DEPT"]);
        var manager = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_ORG", "P_MANAGED_PROJECT"]);
        var broken = AddRole(f.Ctx, ["Expense.Delete"], ["P_DOES_NOT_EXIST"]);
        var users = new[]
        {
            await AddUserAsync(f.Ctx, [cfo], OrgA, DeptAcc, approveLimit: 10_000_000m),
            await AddUserAsync(f.Ctx, [employee], OrgA, DeptTech),
            await AddUserAsync(f.Ctx, [employee, cfo], OrgB, DeptSales, approveLimit: 500_000m, tenantId: Tenant2),
            await AddUserAsync(f.Ctx, [manager], OrgA, managedDepartmentIds: $"{DeptAcc},{DeptSales}"),
            await AddUserAsync(f.Ctx, [broken, employee], null, DeptAcc),
        };

        foreach (var userId in users)
        {
            var rows = Rows(userId);
            foreach (var permission in new[] { "Expense.View", "Expense.Approve", "Expense.Delete" })
            {
                var expected = new List<string>();
                foreach (var row in rows)
                {
                    if (await f.Engine.AuthorizeAsync(userId, permission, row))
                        expected.Add(row.Code);
                }

                Assert.Equal(expected.OrderBy(c => c, StringComparer.Ordinal), await ScopeAsync(f, userId, permission, rows));
            }
        }
    }

    private sealed class NpgsqlScopeContext(DbContextOptions<NpgsqlScopeContext> options) : DbContext(options)
    {
        public DbSet<ExpenseRow> Expenses => Set<ExpenseRow>();
    }

    private static NpgsqlScopeContext NpgsqlContext() =>
        new(new DbContextOptionsBuilder<NpgsqlScopeContext>()
            .UseNpgsql("Host=localhost;Database=scope_translation_only")
            .Options);

    [Fact(DisplayName = "T10: scope translates to SQL WHERE with parameters (no client-side evaluation)")]
    public async Task Scope_TranslatesToSql()
    {
        var f = await CreateAsync();
        var employee = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_TENANT", "P_OWNER_OR_SAME_DEPT"]);
        var manager = AddRole(f.Ctx, ["Expense.View"], ["P_SAME_ORG", "P_APPROVE_LIMIT", "P_MANAGED_PROJECT"]);
        var userId = await AddUserAsync(f.Ctx, [employee, manager], OrgA, DeptTech, approveLimit: 10_000_000m, managedDepartmentIds: $"{DeptAcc},{DeptSales}");

        using var db = NpgsqlContext();
        var sql = (await f.Filter.ApplyFilterAsync(db.Expenses, userId, "Expense.View")).ToQueryString();

        Assert.Contains("WHERE", sql);
        Assert.Contains("\"TenantId\"", sql);
        Assert.Contains("\"CreatedBy\"", sql);
        Assert.Contains("\"Amount\" <=", sql);
        Assert.Contains("= ANY (", sql);
        Assert.DoesNotContain(DeptTech.ToString(), sql.Split("SELECT")[^1]); // values are parameters, not inlined
    }

    [Fact(DisplayName = "T10: deny-all scope translates to SQL (no rows loaded)")]
    public async Task DenyAll_TranslatesToSql()
    {
        var f = await CreateAsync();
        var userId = await AddUserAsync(f.Ctx, [], OrgA);

        using var db = NpgsqlContext();
        var sql = (await f.Filter.ApplyFilterAsync(db.Expenses, userId, "Expense.View")).ToQueryString();

        Assert.Contains("WHERE FALSE", sql);
    }
}
