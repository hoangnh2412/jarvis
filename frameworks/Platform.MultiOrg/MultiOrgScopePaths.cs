using Microsoft.AspNetCore.Http;

namespace Platform.MultiOrg;

/// <summary>
/// Path mặc định cần working org cho API.
/// SPA, swagger, health, static không cần <c>X-Org-Id</c>.
/// Tiền tố thêm (realtime, Elsa, …) do <c>UseMultiOrg</c> truyền vào.
/// </summary>
public static class MultiOrgScopePaths
{
    public static bool IgnoresScope(HttpContext context, IReadOnlyList<string> ignoreStartWithPath)
    {
        var value = context.Request.Path.Value;
        if (string.IsNullOrEmpty(value))
            return true;

        if (value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            // Auth probe / login trước khi client biết org working.
            return value.StartsWith("/api/authentication", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var prefix in ignoreStartWithPath)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
