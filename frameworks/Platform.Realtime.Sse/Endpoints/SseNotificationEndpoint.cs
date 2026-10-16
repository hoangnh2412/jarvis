using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Configuration;
using Platform.Realtime.Groups;
using Platform.Realtime.Identity;
using Platform.Realtime.Sse.Services;

namespace Platform.Realtime.Sse.Endpoints;

internal static class SseNotificationEndpoint
{
    public static async Task HandleAsync<TUser, TTenant>(
        HttpContext httpContext,
        RealtimeConnectionIdentityResolver<TUser, TTenant> identityResolver,
        SseConnectionRegistry registry,
        IOptions<PlatformRealtimeOptions> options,
        CancellationToken cancellationToken)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        await TryAuthenticateAccessTokenQueryAsync(httpContext, options.Value.Sse, cancellationToken)
            .ConfigureAwait(false);

        var identity = await identityResolver
            .ResolveAsync(httpContext.User, httpContext, cancellationToken)
            .ConfigureAwait(false);

        if (identity is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";
        httpContext.Response.ContentType = "text/event-stream";

        var connectionId = Guid.NewGuid().ToString("D");
        var groups = new[]
        {
            RealtimeGroupNames.ForUser(identity.UserId),
            RealtimeGroupNames.ForTenant(identity.TenantId)
        };

        var entry = registry.Register(connectionId, groups, httpContext.Response);

        try
        {
            var heartbeatSeconds = Math.Max(1, options.Value.Sse.HeartbeatSeconds);
            using var heartbeatTimer = new PeriodicTimer(TimeSpan.FromSeconds(heartbeatSeconds));

            while (await heartbeatTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    await entry.WriteHeartbeatAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected.
        }
        finally
        {
            registry.Unregister(connectionId);
        }
    }

    private static async Task TryAuthenticateAccessTokenQueryAsync(
        HttpContext httpContext,
        RealtimeSseOptions sseOptions,
        CancellationToken cancellationToken)
    {
        if (!sseOptions.AllowAccessTokenQuery)
            return;

        if (httpContext.User?.Identity?.IsAuthenticated == true)
            return;

        var accessToken = httpContext.Request.Query["access_token"].ToString();
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        httpContext.Request.Headers.Authorization = $"Bearer {accessToken}";
        var authResult = await httpContext.AuthenticateAsync().ConfigureAwait(false);
        if (authResult.Succeeded && authResult.Principal is not null)
            httpContext.User = authResult.Principal;
    }
}
