using Microsoft.AspNetCore.Http;

namespace Sample.Authentication;

/// <summary>Đọc tenant từ header (trước khi UoW resolve) cho auth middleware.</summary>
internal static class TenantAuthContext
{
    public static Guid? TryGetTenantId(HttpContext? httpContext, string tenantHeaderKey = "X-Tenant-Id")
    {
        if (httpContext is null)
            return null;

        if (!httpContext.Request.Headers.TryGetValue(tenantHeaderKey, out var values))
            return null;

        return Guid.TryParse(values.ToString(), out var tenantId) ? tenantId : null;
    }
}
