using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.Extensions;

namespace Platform.Realtime.Identity;

public sealed class RealtimeConnectionIdentityResolver<TUser, TTenant>(
    ICurrentUser<TUser> currentUser,
    ICurrentTenant<TTenant> currentTenant,
    IConfiguration configuration)
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly string _tenantClaimName =
        configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;
    private readonly string _tenantHeaderKey =
        configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";

    public async Task<RealtimeConnectionIdentity?> ResolveAsync(
        ClaimsPrincipal? principal,
        HttpContext? httpContext,
        CancellationToken cancellationToken = default)
    {
        var userId = await ResolveUserIdAsync(principal, cancellationToken).ConfigureAwait(false);
        if (userId is not { } resolvedUserId || resolvedUserId == Guid.Empty)
            return null;

        var tenantId = await ResolveTenantIdAsync(principal, httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (tenantId is not { } resolvedTenantId || resolvedTenantId == Guid.Empty)
            return null;

        return new RealtimeConnectionIdentity(resolvedUserId, resolvedTenantId);
    }

    private async Task<Guid?> ResolveUserIdAsync(
        ClaimsPrincipal? principal,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        if (user?.UserId is { } userId && userId != Guid.Empty)
            return userId;

        return TryGetUserIdFromPrincipal(principal, out var claimUserId) ? claimUserId : null;
    }

    private async Task<Guid?> ResolveTenantIdAsync(
        ClaimsPrincipal? principal,
        HttpContext? httpContext,
        CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is { } resolved && resolved != Guid.Empty)
            return resolved;

        if (TryGetTenantIdFromPrincipal(principal, out var claimTenantId))
            return claimTenantId;

        return TryGetTenantIdFromRequest(httpContext, out var headerTenantId) ? headerTenantId : null;
    }

    private static bool TryGetUserIdFromPrincipal(ClaimsPrincipal? principal, out Guid userId)
    {
        userId = Guid.Empty;
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        foreach (var name in new[] { ClaimTypes.NameIdentifier, "sub" })
        {
            var value = ClaimsPrincipalExtension.GetClaim(principal.Claims, name)?.Value;
            if (Guid.TryParse(value, out userId) && userId != Guid.Empty)
                return true;
        }

        return false;
    }

    private bool TryGetTenantIdFromPrincipal(ClaimsPrincipal? principal, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        foreach (var name in new[] { _tenantClaimName, "tenant_id" })
        {
            var value = ClaimsPrincipalExtension.GetClaim(principal.Claims, name)?.Value;
            if (TryParseTenantId(value, out tenantId))
                return true;
        }

        return false;
    }

    private bool TryGetTenantIdFromRequest(HttpContext? httpContext, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (httpContext == null)
            return false;

        var raw = httpContext.Request.Headers[_tenantHeaderKey].ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        foreach (var segment in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseTenantId(segment, out tenantId))
                return true;
        }

        return false;
    }

    private static bool TryParseTenantId(string? value, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Guid.TryParse(value, out tenantId) && tenantId != Guid.Empty;
    }
}
