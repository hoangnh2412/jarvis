namespace Jarvis.Modules.Notifications.Contracts;

/// <summary>
/// Đọc policy inbox (Retention / Redis retry) — nguồn SettingDefinition + <c>ISettingManager</c>.
/// </summary>
public interface INotificationInboxSettings
{
    Task<int> GetRetentionDaysAsync(CancellationToken cancellationToken = default);

    Task<int> GetRedisRetryCountAsync(CancellationToken cancellationToken = default);
}
