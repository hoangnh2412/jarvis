using Platform.MultiOrg.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Platform.MultiOrg;

public static class HostApplicationBuilderWebExtensions
{
    /// <summary>Resolve org trước tenant và gắn accessor cho request.</summary>
    public static IApplicationBuilder UseOrgFirstScope(this IApplicationBuilder app) =>
        app.UseMiddleware<OrgFirstScopeMiddleware>();
}
