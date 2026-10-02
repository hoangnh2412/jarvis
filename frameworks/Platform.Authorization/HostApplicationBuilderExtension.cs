namespace Platform.Authorization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Operands;
using Platform.Authorization.Services;

/// <summary>
/// Mở rộng để kết nối Platform.Authorization (phân quyền) vào Host.
/// Providers (permission / policy / operand / governance) đến từ gói kho lưu trữ,
/// ví dụ: <c>AddPlatformAuthorizationEntityFramework()</c> trong Platform.Authorization.EntityFramework.
/// </summary>
public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Đăng ký engine không phụ thuộc vào kho lưu trữ.
    /// </summary>
    public static IHostApplicationBuilder AddPlatformAuthorization(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<IOperandRegistry>(sp => OperandRegistry.Build(sp.GetServices<OperandContribution>()));
        services.AddScoped<IPolicyEvaluator, PolicyEvaluator>();
        services.AddScoped<IAuthorizationEngine, AuthorizationEngine>();
        services.AddScoped<IDataScopeFilter, DataScopeFilter>();
        services.AddScoped<OperandValidator>();

        return builder;
    }

    /// <summary>
    /// Đóng góp các operand kinh doanh (ADR C7). Có thể được gọi bởi một số mô-đun; platform tự nó
    /// chỉ đăng ký các operand chung (User.Id/TenantId/OrgId, Resource.TenantId/OrgId/CreatedBy).
    /// </summary>
    public static IHostApplicationBuilder AddPlatformAuthorizationOperands(
        this IHostApplicationBuilder builder,
        Action<OperandCatalog> configure)
    {
        builder.Services.AddSingleton(new OperandContribution(configure));
        return builder;
    }

    /// <summary>
    /// Cấu hình cache cho các chính sách (giai đoạn 7 - chưa được triển khai).
    /// </summary>
    public static IHostApplicationBuilder AddPlatformAuthorizationCaching(
        this IHostApplicationBuilder builder)
    {
        // TODO: Triển khai trong giai đoạn 7
        // var services = builder.Services;
        // Bao bọc PolicyProvider bằng decorator caching
        // services.Decorate<IPolicyProvider, CachedPolicyProvider>();

        return builder;
    }
}
