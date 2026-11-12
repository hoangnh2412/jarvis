namespace Platform.Authorization.EntityFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.EntityFramework.Services;
using Platform.ORM.EntityFramework.DataSeeding;

/// <summary>
/// Kho lưu trữ EF Core cho Platform.Authorization. Yêu cầu sản phẩm phải đăng ký
/// <see cref="Persistence.IdentityDbContextBase"/> (ví dụ: một lớp con của cơ sở dữ liệu Danh tính được chia sẻ).
/// </summary>
public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Đăng ký các provider được hỗ trợ bởi EF cho quyền hạn, chính sách, operand và quản trị vai trò.
    /// Gọi cùng với <c>AddPlatformAuthorization()</c>.
    /// </summary>
    public static IHostApplicationBuilder AddPlatformAuthorizationEntityFramework(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddScoped<IPolicyProvider, PolicyProvider>();
        services.AddScoped<IOperandResolver, OperandResolver>();
        services.AddScoped<IRoleGovernanceValidator, RoleGovernanceValidator>();
        services.AddScoped<AuthorizationDataSeeder>();
        services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<AuthorizationDataSeeder>());

        return builder;
    }

    /// <summary>
    /// Đóng góp role / policy nghiệp vụ cần seed (ADR V5). Platform tự nó chỉ seed role quản trị
    /// và policy chung (SAME_TENANT, SAME_ORG, OWNER_ONLY). Seed chạy khi Host gọi
    /// <see cref="AuthorizationDataSeeder.SeedAsync"/>.
    /// </summary>
    public static IHostApplicationBuilder AddPlatformAuthorizationSeed(
        this IHostApplicationBuilder builder,
        Action<AuthorizationSeedCatalog> configure)
    {
        builder.Services.AddSingleton(new AuthorizationSeedContribution(configure));
        return builder;
    }
}
