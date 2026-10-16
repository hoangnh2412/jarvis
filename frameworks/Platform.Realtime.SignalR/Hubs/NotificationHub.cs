using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Groups;
using Platform.Realtime.Identity;

namespace Platform.Realtime.SignalR.Hubs;

/// <summary>
/// Hub inbox tại <c>/hubs/notifications</c> (ADR D6). Join group tenant + user khi kết nối.
/// </summary>
public class NotificationHub<TUser, TTenant>(
    RealtimeConnectionIdentityResolver<TUser, TTenant> identityResolver,
    ILogger<NotificationHub<TUser, TTenant>> logger) : Hub
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    public override async Task OnConnectedAsync()
    {
        var identity = await identityResolver.ResolveAsync(
            Context.User,
            Context.GetHttpContext(),
            Context.ConnectionAborted).ConfigureAwait(false);

        if (identity is null)
        {
            logger.LogWarning("Notification hub connection aborted: user and tenant context are required.");
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.ForTenant(identity.TenantId));
        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.ForUser(identity.UserId));

        await base.OnConnectedAsync();
    }
}
