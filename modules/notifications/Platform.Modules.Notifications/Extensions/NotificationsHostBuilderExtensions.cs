using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Services;
using Platform.Modules.Notifications.Contracts;
using Platform.Modules.Notifications.Services;
using Platform.Realtime.Hosting;

namespace Platform.Modules.Notifications.Extensions;

public static class NotificationsHostBuilderExtensions
{
    /// <summary>
    /// Đăng ký <see cref="INotificationAppService"/> trên pipeline <see cref="RealtimeBuilder"/>.
    /// Host phải đăng ký current user/tenant, <c>UseSignalR</c>, và <c>UseRedisInboxStore</c> trước.
    /// </summary>
    public static RealtimeBuilder AddNotificationAppService<TUser, TTenant>(
        this RealtimeBuilder realtimeBuilder)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(realtimeBuilder);
        realtimeBuilder.HostBuilder.Services.TryAddScoped<
            INotificationAppService,
            NotificationAppService<TUser, TTenant>>();
        return realtimeBuilder;
    }
}
