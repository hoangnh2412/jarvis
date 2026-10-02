using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Configuration;
using Platform.Realtime.SignalR.Hubs;

namespace Platform.Realtime.SignalR.Extensions;

/// <summary>
/// Gọi sau UseAuthentication / UseAuthorization.
/// </summary>
public static class RealtimeSignalRApplicationBuilderExtensions
{
    /// <summary>
    /// Map <see cref="NotificationHub{TUser, TTenant}"/> theo hub path đã cấu hình.
    /// </summary>
    public static IEndpointRouteBuilder MapRealtimeHub<TUser, TTenant>(
        this IEndpointRouteBuilder endpoints)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        var options = endpoints.ServiceProvider
            .GetRequiredService<IOptions<PlatformRealtimeOptions>>().Value;

        var path = string.IsNullOrWhiteSpace(options.HubPath)
            ? new PlatformRealtimeOptions().HubPath
            : options.HubPath;

        endpoints.MapHub<NotificationHub<TUser, TTenant>>(path);
        return endpoints;
    }
}
