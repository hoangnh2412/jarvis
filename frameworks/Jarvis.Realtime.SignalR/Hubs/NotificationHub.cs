using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Jarvis.DDD.Domain.Services;
using Jarvis.DDD.Domain.Shared.Extensions;
using Jarvis.Realtime.SignalR.Groups;

namespace Jarvis.Realtime.SignalR.Hubs;

/// <summary>
/// Hub inbox tại <c>/hubs/notifications</c> (ADR D6). Join group tenant + user khi kết nối.
/// </summary>
public class NotificationHub<TUser, TTenant>(
    ICurrentUser<TUser> currentUser,
    ICurrentTenant<TTenant> currentTenant,
    IConfiguration configuration,
    ILogger<NotificationHub<TUser, TTenant>> logger) : Hub
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly string _tenantClaimName =
        configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;
    private readonly string _tenantHeaderKey =
        configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";

    public override async Task OnConnectedAsync()
    {
        var userId = await ResolveUserIdAsync(Context.ConnectionAborted).ConfigureAwait(false);
        if (userId is not { } resolvedUserId || resolvedUserId == Guid.Empty)
        {
            logger.LogWarning("Notification hub connection aborted: user context is required.");
            Context.Abort();
            return;
        }

        var tenantId = await ResolveTenantIdAsync(Context.ConnectionAborted).ConfigureAwait(false);
        if (tenantId is not { } resolvedTenantId || resolvedTenantId == Guid.Empty)
        {
            logger.LogWarning("Notification hub connection aborted: tenant context is required.");
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.ForTenant(resolvedTenantId));
        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.ForUser(resolvedUserId));

        await base.OnConnectedAsync();
    }

    private async Task<Guid?> ResolveUserIdAsync(CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        if (user?.UserId is { } userId && userId != Guid.Empty)
            return userId;

        return TryGetUserIdFromPrincipal(Context.User, out var claimUserId) ? claimUserId : null;
    }

    private async Task<Guid?> ResolveTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is { } resolved && resolved != Guid.Empty)
            return resolved;

        if (TryGetTenantIdFromPrincipal(Context.User, out var claimTenantId))
            return claimTenantId;

        return TryGetTenantIdFromHubRequest(out var headerTenantId) ? headerTenantId : null;
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

    private bool TryGetTenantIdFromHubRequest(out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var httpContext = Context.GetHttpContext();
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
