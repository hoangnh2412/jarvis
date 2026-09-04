namespace Jarvis.Modules.Notifications.Configuration;

/// <summary>
/// Tùy chọn infra inbox Redis — <see cref="InstanceName"/> từ
/// <c>Cache:DistributedGroups:Redis:Notifications</c>.
/// Retention / retry lấy qua Setting module (<c>NotificationInbox.*</c>), không nằm ở đây.
/// </summary>
public sealed class NotificationInboxOptions
{
    /// <summary>Prefix key Redis (= Cache InstanceName).</summary>
    public string InstanceName { get; set; } = string.Empty;
}
