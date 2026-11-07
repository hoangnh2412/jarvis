using Platform.MultiOrg.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Platform.MultiOrg;

public static class HostApplicationBuilderWebExtensions
{
    /// <summary>Gắn working org và tenant đã resolve cho một request.</summary>
    /// <param name="app">Pipeline ứng dụng.</param>
    /// <param name="ignore">
    /// Thay <see cref="MultiOrgScopePaths.IgnoresScope"/>.
    /// <see langword="null"/> giữ mặc định. Trả về <see langword="true"/> thì request không cần working scope.
    /// </param>
    /// <param name="ignoreStartWithPaths">
    /// Tiền tố path. Request bắt đầu bằng một tiền tố thì không bỏ qua (cần working org), ngoài <c>/api/</c>.
    /// <see langword="null"/> thì chỉ áp quy tắc API.
    /// </param>
    public static IApplicationBuilder UseMultiOrg(
        this IApplicationBuilder app,
        Func<HttpContext, bool>? ignore = null,
        IReadOnlyList<string>? ignoreStartWithPaths = null)
    {
        if (ignore is not null)
            return app.UseMiddleware<MultiOrgMiddleware>(ignore);

        var prefixes = ignoreStartWithPaths ?? [];
        Func<HttpContext, bool> ignoreScope = context => MultiOrgScopePaths.IgnoresScope(context, prefixes);
        return app.UseMiddleware<MultiOrgMiddleware>(ignoreScope);
    }
}
