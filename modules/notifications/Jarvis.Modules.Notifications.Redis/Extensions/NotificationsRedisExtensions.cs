using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Jarvis.Caching;
using Jarvis.Modules.Notifications.Configuration;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Redis.Store;
using Jarvis.Modules.Notifications.Services;
using Jarvis.Modules.Setting.Services;
using Jarvis.Realtime.Hosting;
using StackExchange.Redis;

namespace Jarvis.Modules.Notifications.Redis.Extensions;

public static class NotificationsRedisExtensions
{
    public const string CacheRedisGroupName = "Notifications";

    /// <summary>
    /// Đăng ký Redis inbox store từ <c>Cache:DistributedGroups:Redis:Notifications</c>.
    /// Fail-fast nếu thiếu group / Configuration / InstanceName.
    /// Retention / retry: Setting group <c>NotificationInbox</c> (qua <see cref="INotificationInboxSettings"/>).
    /// </summary>
    public static RealtimeBuilder UseRedisInboxStore(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var cacheOpts = new JarvisCacheOptions();
        builder.HostBuilder.Configuration.GetSection(JarvisCacheOptions.SectionName).Bind(cacheOpts);

        if (!cacheOpts.DistributedGroups.TryGetValue("Redis", out var redisSection)
            || !redisSection.TryGetValue(CacheRedisGroupName, out var notificationsGroup))
        {
            throw new InvalidOperationException(
                "Cache:DistributedGroups:Redis:Notifications is required when calling UseRedisInboxStore().");
        }

        if (!notificationsGroup.TryGetValue("Configuration", out var redisConfig)
            || string.IsNullOrWhiteSpace(redisConfig))
        {
            throw new InvalidOperationException(
                "Cache:DistributedGroups:Redis:Notifications:Configuration is required when calling UseRedisInboxStore().");
        }

        if (!notificationsGroup.TryGetValue("InstanceName", out var instanceName)
            || string.IsNullOrWhiteSpace(instanceName))
        {
            throw new InvalidOperationException(
                "Cache:DistributedGroups:Redis:Notifications:InstanceName is required when calling UseRedisInboxStore().");
        }

        // InstanceName chỉ từ Cache group (appsettings) — không Retention/Retry ở đây.
        builder.HostBuilder.Services.Configure<NotificationInboxOptions>(opt =>
        {
            opt.InstanceName = instanceName;
        });

        builder.HostBuilder.Services.TryAddScoped<INotificationInboxSettings>(sp =>
        {
            var settingManager = sp.GetService<ISettingManager>();
            return settingManager is null
                ? new DefaultNotificationInboxSettings()
                : new SettingNotificationInboxSettings(settingManager);
        });

        builder.HostBuilder.Services.TryAddKeyedSingleton<IConnectionMultiplexer>(
            RedisNotificationDefaults.StoreConnectionServiceKey,
            (_, _) => ConnectStoreMultiplexer(redisConfig));

        builder.HostBuilder.Services.RemoveAll<INotificationStore>();

        // Scoped: cùng lifetime với ISettingManager / INotificationAppService.
        builder.HostBuilder.Services.AddScoped<INotificationStore>(sp =>
            new RedisNotificationStore(
                sp.GetRequiredKeyedService<IConnectionMultiplexer>(RedisNotificationDefaults.StoreConnectionServiceKey),
                sp.GetRequiredService<IOptions<NotificationInboxOptions>>(),
                sp.GetRequiredService<INotificationInboxSettings>()));

        return builder;
    }

    private static ConnectionMultiplexer ConnectStoreMultiplexer(string configuration)
        => ConnectionMultiplexer.Connect(configuration);
}
