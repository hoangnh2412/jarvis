using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Operands;

namespace Sample.AuthorizationDemo;

/// <summary>Tài nguyên demo: thuộc tính public được đọc qua operand <c>Resource.*</c>.</summary>
public sealed record SampleExpense(Guid TenantId, Guid DepartmentId, decimal Amount, Guid CreatedBy);

/// <summary>Chứng từ chi phí lưu trong bảng <c>SampleExpenses</c> — dùng cho demo Data Scope.</summary>
public sealed class SampleExpenseRow
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid DepartmentId { get; set; }
    public decimal Amount { get; set; }
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// Dữ liệu demo phân quyền của Sample: operand, role/policy nghiệp vụ (đóng góp vào seeder platform),
/// role claims, user mẫu và bộ kịch bản kiểm thử trực tiếp.
/// </summary>
public static class SampleAuthorizationDemoData
{
    public const string PermissionClaim = "Permission";
    public const string PolicyClaim = "Policy";

    public static readonly IReadOnlyDictionary<string, Guid> Tenants = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
    {
        ["T1"] = Guid.Parse("40000000-0000-0000-0000-000000000001"),
        ["T2"] = Guid.Parse("40000000-0000-0000-0000-000000000002"),
    };

    public static readonly IReadOnlyDictionary<string, Guid> Departments = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
    {
        ["Sales"] = Guid.Parse("41000000-0000-0000-0000-000000000001"),
        ["IT"] = Guid.Parse("41000000-0000-0000-0000-000000000002"),
        ["Marketing"] = Guid.Parse("41000000-0000-0000-0000-000000000003"),
    };

    private static readonly Guid AdminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ViewerRoleId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid ApproverRoleId = Guid.Parse("50000000-0000-0000-0000-000000000002");
    private static readonly Guid RegionalRoleId = Guid.Parse("50000000-0000-0000-0000-000000000003");
    private static readonly Guid EmployeeRoleId = Guid.Parse("50000000-0000-0000-0000-000000000004");
    private static readonly Guid MisconfiguredRoleId = Guid.Parse("50000000-0000-0000-0000-000000000005");

    /// <summary>Operand nghiệp vụ Sample đăng ký thêm vào registry (ADR C7).</summary>
    public static void ConfigureOperands(OperandCatalog operands) =>
        operands
            .AddUserClaim("DepartmentId")
            .AddUserClaim("ApproveLimit")
            .AddUserClaim("ManagedDepartmentIds", OperandValueKind.Collection)
            .AddResource("DepartmentId")
            .AddResource("Amount");

    /// <summary>Role / policy nghiệp vụ Sample đóng góp vào seeder platform (ADR V5).</summary>
    public static void ConfigureSeed(AuthorizationSeedCatalog seed) =>
        seed
            .AddRole(NewRole(ViewerRoleId, "SAMPLE_EXPENSE_VIEWER", 200))
            .AddRole(NewRole(ApproverRoleId, "SAMPLE_EXPENSE_APPROVER", 500))
            .AddRole(NewRole(RegionalRoleId, "SAMPLE_REGIONAL_MANAGER", 600))
            .AddRole(NewRole(EmployeeRoleId, "SAMPLE_EMPLOYEE", 100))
            .AddRole(NewRole(MisconfiguredRoleId, "SAMPLE_MISCONFIGURED", 100))
            .AddPolicy(NewPolicy("51000000-0000-0000-0000-000000000001", "SAMPLE_SAME_DEPARTMENT", "Cùng phòng ban",
                NewRule("52000000-0000-0000-0000-000000000001", 1, "Resource.DepartmentId", "==", "User.DepartmentId")))
            .AddPolicy(NewPolicy("51000000-0000-0000-0000-000000000002", "SAMPLE_APPROVE_LIMIT", "Trong hạn mức duyệt",
                NewRule("52000000-0000-0000-0000-000000000002", 1, "Resource.Amount", "<=", "User.ApproveLimit")))
            .AddPolicy(NewPolicy("51000000-0000-0000-0000-000000000003", "SAMPLE_MANAGED_DEPARTMENT", "Phòng ban được quản lý",
                NewRule("52000000-0000-0000-0000-000000000003", 1, "Resource.DepartmentId", "IN", "User.ManagedDepartmentIds")));

    /// <summary>Role → (permission, policy). Policy AND trong một role; các role cấp permission OR với nhau.</summary>
    private static readonly (Guid RoleId, string[] Permissions, string[] Policies)[] RoleGrants =
    [
        (AdminRoleId, ["*"], []),
        (ViewerRoleId, ["Expense.View"], ["P_SAME_TENANT", "SAMPLE_SAME_DEPARTMENT"]),
        (ApproverRoleId, ["Expense.Approve"], ["P_SAME_TENANT", "SAMPLE_SAME_DEPARTMENT", "SAMPLE_APPROVE_LIMIT"]),
        (RegionalRoleId, ["Expense.View"], ["P_SAME_TENANT", "SAMPLE_MANAGED_DEPARTMENT"]),
        (EmployeeRoleId, ["Expense.View"], ["P_OWNER_ONLY"]),
        (MisconfiguredRoleId, ["Expense.Delete"], ["SAMPLE_POLICY_NOT_SEEDED"]),
    ];

    public sealed record DemoUser(Guid Id, string UserName, string Tenant, Guid[] Roles, IReadOnlyDictionary<string, string> Claims);

    public static readonly IReadOnlyList<DemoUser> Users =
    [
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"), "admin", "T1", [AdminRoleId], new Dictionary<string, string>()),
        new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "manager.sales", "T1", [ViewerRoleId, ApproverRoleId],
            new Dictionary<string, string> { ["DepartmentId"] = Dept("Sales"), ["ApproveLimit"] = Money(10_000_000m) }),
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"), "manager.it", "T1", [ViewerRoleId, ApproverRoleId],
            new Dictionary<string, string> { ["DepartmentId"] = Dept("IT"), ["ApproveLimit"] = Money(5_000_000m) }),
        new(Guid.Parse("30000000-0000-0000-0000-000000000004"), "regional", "T1", [RegionalRoleId],
            new Dictionary<string, string> { ["ManagedDepartmentIds"] = $"{Dept("Sales")},{Dept("IT")}" }),
        new(Guid.Parse("30000000-0000-0000-0000-000000000005"), "employee", "T1", [EmployeeRoleId],
            new Dictionary<string, string> { ["DepartmentId"] = Dept("Sales") }),
        new(Guid.Parse("30000000-0000-0000-0000-000000000006"), "other.tenant", "T2", [ViewerRoleId, ApproverRoleId],
            new Dictionary<string, string> { ["DepartmentId"] = Dept("Sales"), ["ApproveLimit"] = Money(100_000_000m) }),
        new(Guid.Parse("30000000-0000-0000-0000-000000000007"), "misconfigured", "T1", [MisconfiguredRoleId], new Dictionary<string, string>()),
    ];

    public sealed record Scenario(string Id, string Description, string User, string Permission, string Tenant, string Department, decimal Amount, string CreatedBy, bool ExpectedAllowed);

    public static readonly IReadOnlyList<Scenario> Scenarios =
    [
        new("AZ-01", "Admin có '*' → bỏ qua ABAC", "admin", "Expense.Delete", "T2", "Marketing", 999_000_000m, "employee", true),
        new("AZ-02", "Duyệt cùng tenant + phòng ban, 8M ≤ hạn mức 10M", "manager.sales", "Expense.Approve", "T1", "Sales", 8_000_000m, "employee", true),
        new("AZ-03", "Duyệt vượt hạn mức 15M > 10M", "manager.sales", "Expense.Approve", "T1", "Sales", 15_000_000m, "employee", false),
        new("AZ-04", "Duyệt phòng ban khác dù trong hạn mức", "manager.sales", "Expense.Approve", "T1", "IT", 1_000_000m, "employee", false),
        new("AZ-05", "Xem 15M: role Viewer không xét hạn mức (path theo role)", "manager.sales", "Expense.View", "T1", "Sales", 15_000_000m, "employee", true),
        new("AZ-06", "Hạn mức 5M, chứng từ 6M", "manager.it", "Expense.Approve", "T1", "IT", 6_000_000m, "employee", false),
        new("AZ-07", "Khác tenant dù hạn mức 100M", "other.tenant", "Expense.Approve", "T1", "Sales", 1_000_000m, "employee", false),
        new("AZ-08", "IN: IT thuộc danh sách phòng ban được quản lý", "regional", "Expense.View", "T1", "IT", 1_000_000m, "employee", true),
        new("AZ-09", "IN: Marketing không thuộc danh sách quản lý", "regional", "Expense.View", "T1", "Marketing", 1_000_000m, "employee", false),
        new("AZ-10", "RBAC: không có Expense.Approve", "regional", "Expense.Approve", "T1", "IT", 1_000_000m, "employee", false),
        new("AZ-11", "Owner: chứng từ do chính mình tạo", "employee", "Expense.View", "T1", "Marketing", 1_000_000m, "employee", true),
        new("AZ-12", "Owner: chứng từ người khác tạo", "employee", "Expense.View", "T1", "Sales", 1_000_000m, "manager.sales", false),
        new("AZ-13", "Role gắn mã policy chưa seed → fail-closed", "misconfigured", "Expense.Delete", "T1", "Sales", 1m, "misconfigured", false),
    ];

    /// <summary>Chứng từ demo: (mã, tenant, phòng ban, số tiền, người tạo).</summary>
    private static readonly (string Code, string Tenant, string Department, decimal Amount, string CreatedBy)[] ExpenseSeed =
    [
        ("E-01", "T1", "Sales", 8_000_000m, "employee"),
        ("E-02", "T1", "Sales", 15_000_000m, "manager.sales"),
        ("E-03", "T1", "IT", 1_000_000m, "manager.it"),
        ("E-04", "T1", "IT", 6_000_000m, "employee"),
        ("E-05", "T1", "Marketing", 2_000_000m, "employee"),
        ("E-06", "T1", "Marketing", 3_000_000m, "manager.sales"),
        ("E-07", "T2", "Sales", 1_000_000m, "other.tenant"),
    ];

    /// <summary>Permission được kiểm tra chéo giữa Data Scope và engine.</summary>
    public static readonly string[] ScopePermissions = ["Expense.View", "Expense.Approve", "Expense.Delete"];

    public static DemoUser? FindUser(string userName) =>
        Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Thêm role claims, user, user roles, user claims còn thiếu (idempotent).</summary>
    public static async Task EnsureDemoDataAsync(IdentityDbContextBase context, CancellationToken cancellationToken = default)
    {
        var existingRoleClaims = (await context.RoleClaims
                .Select(rc => new { rc.RoleId, rc.ClaimType, rc.ClaimValue })
                .ToListAsync(cancellationToken))
            .Select(rc => (rc.RoleId, rc.ClaimType, rc.ClaimValue))
            .ToHashSet();

        foreach (var (roleId, permissions, policies) in RoleGrants)
        {
            var claims = permissions.Select(p => (PermissionClaim, p)).Concat(policies.Select(p => (PolicyClaim, p)));
            foreach (var (type, value) in claims)
            {
                if (existingRoleClaims.Add((roleId, type, value)))
                    context.RoleClaims.Add(new IdentityRoleClaim<Guid> { RoleId = roleId, ClaimType = type, ClaimValue = value });
            }
        }

        var existingUsers = (await context.Users.Select(u => u.Id).ToListAsync(cancellationToken)).ToHashSet();
        foreach (var user in Users.Where(u => !existingUsers.Contains(u.Id)))
        {
            context.Users.Add(new User
            {
                Id = user.Id,
                UserName = user.UserName,
                NormalizedUserName = user.UserName.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                TenantId = Tenants[user.Tenant],
            });
            foreach (var roleId in user.Roles)
                context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleId });
            foreach (var (type, value) in user.Claims)
                context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = user.Id, ClaimType = type, ClaimValue = value });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Thêm chứng từ demo còn thiếu (theo Code).</summary>
    public static async Task EnsureExpensesAsync(SampleAuthorizationDbContext context, CancellationToken cancellationToken = default)
    {
        var existing = (await context.Expenses.Select(e => e.Code).ToListAsync(cancellationToken)).ToHashSet();
        foreach (var (code, tenant, department, amount, createdBy) in ExpenseSeed.Where(e => !existing.Contains(e.Code)))
        {
            context.Expenses.Add(new SampleExpenseRow
            {
                Id = Guid.NewGuid(),
                Code = code,
                TenantId = Tenants[tenant],
                DepartmentId = Departments[department],
                Amount = amount,
                CreatedBy = FindUser(createdBy)!.Id,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string Dept(string name) => Departments[name].ToString();

    private static string Money(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static Role NewRole(Guid id, string name, int level) =>
        new() { Id = id, Name = name, NormalizedName = name, DisplayName = name, RoleLevel = level, IsSystemRole = false };

    private static Policy NewPolicy(string id, string code, string name, params PolicyRule[] rules) =>
        new() { Id = Guid.Parse(id), Code = code, Name = name, IsSystemPolicy = false, Rules = rules.ToList() };

    private static PolicyRule NewRule(string id, int order, string left, string op, string right) =>
        new() { Id = Guid.Parse(id), Order = order, LeftOperand = left, Operator = op, RightOperand = right };
}
