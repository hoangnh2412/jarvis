using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Definitions;

namespace Jarvis.Modules.Notifications.Services;

/// <summary>
/// Fallback khi host chưa đăng ký Setting — dùng default trên <see cref="NotificationInboxSettingDefinition"/>.
/// </summary>
public sealed class DefaultNotificationInboxSettings : INotificationInboxSettings
{
    public Task<int> GetRetentionDaysAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(NotificationInboxSettingDefinition.DefaultRetentionDays);

    public Task<int> GetRedisRetryCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(NotificationInboxSettingDefinition.DefaultRedisRetryCount);
}
