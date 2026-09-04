using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Definitions;
using Jarvis.Modules.Setting.Services;

namespace Jarvis.Modules.Notifications.Services;

/// <summary>
/// Đọc <c>NotificationInbox.*</c> qua <see cref="ISettingManager.GetOrCreateAsync"/> (default từ definition).
/// </summary>
public sealed class SettingNotificationInboxSettings(ISettingManager settingManager) : INotificationInboxSettings
{
    public async Task<int> GetRetentionDaysAsync(CancellationToken cancellationToken = default)
    {
        var model = await settingManager
            .GetOrCreateAsync(NotificationInboxSettingDefinition.RetentionDaysKey, cancellationToken)
            .ConfigureAwait(false);

        return ParsePositiveOrDefault(model.Value, NotificationInboxSettingDefinition.DefaultRetentionDays);
    }

    public async Task<int> GetRedisRetryCountAsync(CancellationToken cancellationToken = default)
    {
        var model = await settingManager
            .GetOrCreateAsync(NotificationInboxSettingDefinition.RedisRetryCountKey, cancellationToken)
            .ConfigureAwait(false);

        return ParseNonNegativeOrDefault(model.Value, NotificationInboxSettingDefinition.DefaultRedisRetryCount);
    }

    private static int ParsePositiveOrDefault(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : defaultValue;

    private static int ParseNonNegativeOrDefault(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) && parsed >= 0 ? parsed : defaultValue;
}
