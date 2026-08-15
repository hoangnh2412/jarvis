using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Jarvis.DDD.Domain.Services;
using Jarvis.Realtime.Configuration;
using Jarvis.Realtime.SignalR.Hubs;

namespace Jarvis.Realtime.SignalR.Extensions;

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
            .GetRequiredService<IOptions<JarvisRealtimeOptions>>().Value;

        var path = string.IsNullOrWhiteSpace(options.HubPath)
            ? new JarvisRealtimeOptions().HubPath
            : options.HubPath;

        endpoints.MapHub<NotificationHub<TUser, TTenant>>(path);
        return endpoints;
    }
}
