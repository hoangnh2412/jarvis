using Platform.DDD.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace Platform.MultiOrg.Middleware;

/// <summary>Áp working org/tenant lên accessor theo <see cref="IRequestScopeResolver"/> (một request).</summary>
public sealed class MultiOrgMiddleware(
    RequestDelegate next,
    Func<HttpContext, bool> ignore)
{
    public async Task InvokeAsync(
        HttpContext context,
        IRequestScopeResolver scopeResolver,
        ICurrentOrgAccessor orgAccessor,
        ICurrentTenantAccessor tenantAccessor)
    {
        if (ignore(context))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var resolution = await scopeResolver.ResolveAsync(context.RequestAborted).ConfigureAwait(false);
        if (resolution.IsForbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (!resolution.IsAllowed)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        using var orgScope = orgAccessor.BeginScope(resolution.OrgId!.Value);
        using var tenantScope = tenantAccessor.BeginScope(resolution.TenantId!.Value);
        await next(context).ConfigureAwait(false);
    }
}
