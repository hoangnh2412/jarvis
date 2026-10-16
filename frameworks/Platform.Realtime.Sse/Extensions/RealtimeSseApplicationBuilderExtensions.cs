using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Configuration;
using Platform.Realtime.Identity;
using Platform.Realtime.Sse.Endpoints;
using Platform.Realtime.Sse.Services;

namespace Platform.Realtime.Sse.Extensions;

/// <summary>
/// Gọi sau UseAuthentication / UseAuthorization.
/// </summary>
public static class RealtimeSseApplicationBuilderExtensions
{
    /// <summary>
    /// Map SSE notifications endpoint theo <c>Realtime:Sse:SsePath</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapRealtimeSse<TUser, TTenant>(
        this IEndpointRouteBuilder endpoints)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<PlatformRealtimeOptions>>().Value;
        var path = string.IsNullOrWhiteSpace(options.Sse.SsePath)
            ? new RealtimeSseOptions().SsePath
            : options.Sse.SsePath;

        endpoints.MapGet(path, HandleAsync<TUser, TTenant>);
        return endpoints;
    }

    private static Task HandleAsync<TUser, TTenant>(
        HttpContext httpContext,
        RealtimeConnectionIdentityResolver<TUser, TTenant> identityResolver,
        SseConnectionRegistry registry,
        IOptions<PlatformRealtimeOptions> options,
        CancellationToken cancellationToken)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
        => SseNotificationEndpoint.HandleAsync(
            httpContext,
            identityResolver,
            registry,
            options,
            cancellationToken);
}
