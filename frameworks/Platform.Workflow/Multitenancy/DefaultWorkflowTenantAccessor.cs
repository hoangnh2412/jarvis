using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Platform.Workflow.Multitenancy;

/// <summary>
/// Triển khai mặc định của <see cref="IWorkflowTenantAccessor"/>:
/// 1. Tự động tương thích với Platform.DDD.Domain (ICurrentTenantAccessor) qua DI/Reflection nếu có trong runtime.
/// 2. Đọc từ HttpContext headers (mặc định X-Tenant-Id) / claims khi có HTTP request.
/// Hoàn toàn độc lập, không yêu cầu ProjectReference compile-time tới Platform.DDD.Domain.
/// </summary>
public sealed class DefaultWorkflowTenantAccessor(
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : IWorkflowTenantAccessor
{
    private static readonly Type? CurrentTenantAccessorType = FindCurrentTenantAccessorType();
    private static readonly PropertyInfo? TenantIdProperty = CurrentTenantAccessorType?.GetProperty("TenantId");

    public string? GetTenantId()
    {
        // 1. Thử resolve qua Platform ICurrentTenantAccessor nếu có trong DI
        if (CurrentTenantAccessorType != null && TenantIdProperty != null)
        {
            var accessor = serviceProvider.GetService(CurrentTenantAccessorType);
            if (accessor != null)
            {
                var val = TenantIdProperty.GetValue(accessor);
                if (val != null)
                {
                    var idStr = val.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(idStr) && !string.Equals(idStr, Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return idStr;
                    }
                }
            }
        }

        // 2. Fallback đọc từ HttpContext (Header hoặc Claims)
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var headerKey = configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";
            var headerValue = httpContext.Request.Headers[headerKey].ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }

            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value
                              ?? httpContext.User.FindFirst("tenantid")?.Value
                              ?? httpContext.User.FindFirst("tenant")?.Value;
            if (!string.IsNullOrWhiteSpace(tenantClaim))
            {
                return tenantClaim.Trim();
            }
        }

        return null;
    }

    private static Type? FindCurrentTenantAccessorType()
    {
        return Type.GetType("Platform.DDD.Domain.Services.ICurrentTenantAccessor, Platform.DDD.Domain")
               ?? AppDomain.CurrentDomain.GetAssemblies()
                   .Select(a => a.GetType("Platform.DDD.Domain.Services.ICurrentTenantAccessor"))
                   .FirstOrDefault(t => t != null);
    }
}
