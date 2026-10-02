namespace Platform.Authorization.EntityFramework.Persistence;

using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;
using Platform.Authorization.Services;

/// <summary>
/// Seed role và policy vào cơ sở dữ liệu.
/// Platform chỉ seed role quản trị (ADMIN / TENANT_ADMIN / ORG_ADMIN) và policy dùng operand chung;
/// role / policy nghiệp vụ do sản phẩm đóng góp (ADR V5).
/// </summary>
public static class AuthorizationSeeder
{
    /// <summary>
    /// Mặc định của platform trước tiên, sau đó là các đóng góp theo thứ tự đăng ký.
    /// </summary>
    public static AuthorizationSeedCatalog BuildCatalog(IEnumerable<Action<AuthorizationSeedCatalog>> contributions)
    {
        var catalog = new AuthorizationSeedCatalog().AddPlatformDefaults();
        foreach (var configure in contributions)
            configure(catalog);
        return catalog;
    }

    public static AuthorizationSeedCatalog BuildCatalog(params Action<AuthorizationSeedCatalog>[] contributions) =>
        BuildCatalog((IEnumerable<Action<AuthorizationSeedCatalog>>)contributions);

    /// <summary>
    /// Chỉ seed mặc định của platform.
    /// </summary>
    public static Task SeedAsync(this IdentityDbContextBase context, CancellationToken cancellationToken = default) =>
        context.SeedAsync(BuildCatalog(), validator: null, cancellationToken);

    /// <summary>
    /// Seed <paramref name="catalog"/>: chỉ thêm role (theo NormalizedName) và policy (theo Code) chưa có,
    /// không ghi đè bản ghi đã tồn tại. Khi có <paramref name="validator"/>, mọi rule phải dùng operand
    /// đã đăng ký — nếu không sẽ ném lỗi trước khi ghi (thay vì để policy fail-closed lúc chạy).
    /// </summary>
    public static async Task SeedAsync(
        this IdentityDbContextBase context,
        AuthorizationSeedCatalog catalog,
        OperandValidator? validator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (validator != null)
            EnsureRulesValid(catalog, validator);

        var existingRoles = (await context.Roles
                .Select(r => r.NormalizedName)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        var existingPolicies = (await context.Policies
                .Select(p => p.Code)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        await context.Roles.AddRangeAsync(
            catalog.Roles.Where(r => !existingRoles.Contains(r.NormalizedName!)), cancellationToken);
        await context.Policies.AddRangeAsync(
            catalog.Policies.Where(p => !existingPolicies.Contains(p.Code)), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureRulesValid(AuthorizationSeedCatalog catalog, OperandValidator validator)
    {
        var errors = catalog.Policies
            .SelectMany(p => p.Rules.OrderBy(r => r.Order).Select(r => (Policy: p, Rule: r)))
            .Select(x => (x.Policy, x.Rule, Result: validator.ValidateRule(x.Rule.LeftOperand, x.Rule.Operator, x.Rule.RightOperand)))
            .Where(x => !x.Result.IsValid)
            .Select(x => $"{x.Policy.Code}#{x.Rule.Order}: {x.Result.ErrorMessage}")
            .ToList();

        if (errors.Count > 0)
            throw new InvalidOperationException(
                "Seed phân quyền chứa rule không hợp lệ: " + string.Join("; ", errors));
    }

    /// <summary>
    /// Role quản trị và policy chỉ dùng operand mặc định của platform.
    /// </summary>
    public static AuthorizationSeedCatalog AddPlatformDefaults(this AuthorizationSeedCatalog catalog)
    {
        catalog
            .AddRole(new Role
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "ADMIN",
                NormalizedName = "ADMIN",
                DisplayName = "Admin",
                RoleLevel = 1000,
                IsSystemRole = true,
            })
            .AddRole(new Role
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "TENANT_ADMIN",
                NormalizedName = "TENANT_ADMIN",
                DisplayName = "TenantAdmin",
                RoleLevel = 900,
                IsSystemRole = true,
            })
            .AddRole(new Role
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "ORG_ADMIN",
                NormalizedName = "ORG_ADMIN",
                DisplayName = "OrgAdmin",
                RoleLevel = 800,
                IsSystemRole = true,
            });

        catalog
            .AddPolicy(new Policy
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Code = "P_SAME_TENANT",
                Name = "Same Tenant",
                ResourceType = null,
                Action = null,
                IsSystemPolicy = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = Guid.Parse("21000000-0000-0000-0000-000000000001"),
                        Order = 1,
                        LeftOperand = "Resource.TenantId",
                        Operator = "==",
                        RightOperand = "User.TenantId",
                    }
                }
            })
            .AddPolicy(new Policy
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Code = "P_SAME_ORG",
                Name = "Same Organization",
                ResourceType = null,
                Action = null,
                IsSystemPolicy = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = Guid.Parse("21000000-0000-0000-0000-000000000002"),
                        Order = 1,
                        LeftOperand = "Resource.OrgId",
                        Operator = "==",
                        RightOperand = "User.OrgId",
                    }
                }
            })
            .AddPolicy(new Policy
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Code = "P_OWNER_ONLY",
                Name = "Owner Only",
                ResourceType = null,
                Action = null,
                IsSystemPolicy = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = Guid.Parse("21000000-0000-0000-0000-000000000004"),
                        Order = 1,
                        LeftOperand = "Resource.CreatedBy",
                        Operator = "==",
                        RightOperand = "User.Id",
                    }
                }
            });

        return catalog;
    }
}
