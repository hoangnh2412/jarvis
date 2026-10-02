using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;

namespace UnitTest.Authorization.Helpers;

/// <summary>
/// Platform defaults + the business roles/policies a product seeds (mirrors Lexora's
/// AuthorizationSeedExtension, which platform tests cannot reference).
/// </summary>
internal static class TestSeeds
{
    public static void Business(AuthorizationSeedCatalog seed)
    {
        seed
            .AddRole(Role("10000000-0000-0000-0000-000000000004", "DIRECTOR", 700))
            .AddRole(Role("10000000-0000-0000-0000-000000000005", "CFO", 650))
            .AddRole(Role("10000000-0000-0000-0000-000000000006", "DEPARTMENT_MANAGER", 500))
            .AddRole(Role("10000000-0000-0000-0000-000000000007", "EMPLOYEE", 100))
            .AddPolicy(Policy("20000000-0000-0000-0000-000000000003", "P_SAME_DEPARTMENT", "Same Department",
                Rule("21000000-0000-0000-0000-000000000003", 1, "Resource.DepartmentId", "==", "User.DepartmentId")))
            .AddPolicy(Policy("20000000-0000-0000-0000-000000000005", "P_APPROVE_LIMIT", "Approve Limit",
                Rule("21000000-0000-0000-0000-000000000005", 1, "Resource.Amount", "<=", "User.ApproveLimit")))
            .AddPolicy(Policy("20000000-0000-0000-0000-000000000006", "P_MANAGED_PROJECT", "Managed Project",
                Rule("21000000-0000-0000-0000-000000000006", 1, "Resource.DepartmentId", "IN", "User.ManagedDepartmentIds")))
            .AddPolicy(Policy("20000000-0000-0000-0000-000000000007", "P_OWNER_OR_SAME_DEPT", "Owner Or Same Department",
                Rule("21000000-0000-0000-0000-000000000007", 1, "Resource.CreatedBy", "==", "User.Id"),
                Rule("21000000-0000-0000-0000-000000000008", 2, "Resource.DepartmentId", "==", "User.DepartmentId")));
    }

    /// <summary>Seeds platform defaults + business seed, validated against the business operand registry.</summary>
    public static Task SeedWithBusinessAsync(this IdentityDbContextBase context) =>
        context.SeedAsync(AuthorizationSeeder.BuildCatalog(Business), new OperandValidator(TestOperands.Registry()));

    private static Role Role(string id, string name, int level) =>
        new() { Id = Guid.Parse(id), Name = name, NormalizedName = name, DisplayName = name, RoleLevel = level };

    private static Policy Policy(string id, string code, string name, params PolicyRule[] rules) =>
        new() { Id = Guid.Parse(id), Code = code, Name = name, IsSystemPolicy = true, Rules = rules.ToList() };

    private static PolicyRule Rule(string id, int order, string left, string op, string right) =>
        new() { Id = Guid.Parse(id), Order = order, LeftOperand = left, Operator = op, RightOperand = right };
}
