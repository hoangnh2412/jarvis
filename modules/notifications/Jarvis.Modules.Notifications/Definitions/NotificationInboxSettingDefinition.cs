using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Models;

namespace Jarvis.Modules.Notifications.Definitions;

/// <summary>
/// SettingDefinition inbox (group <c>NotificationInbox</c>) — tách khỏi Sample group <c>Notification</c>.
/// </summary>
public sealed class NotificationInboxSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "NotificationInbox";
    public const string RetentionDaysKey = "NotificationInbox.RetentionDays";
    public const string RedisRetryCountKey = "NotificationInbox.RedisRetryCount";

    public const int DefaultRetentionDays = 20;
    public const int DefaultRedisRetryCount = 2;

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Notification inbox";
            group.Description = "TTL và retry Redis cho inbox thông báo";
            group.Order = 40;
        });

        context.AddSetting(GroupName, RetentionDaysKey, setting =>
        {
            setting.Name = "Retention days";
            setting.Description = "Số ngày giữ item Redis (TTL lúc Save)";
            setting.Type = SettingValueTypes.Number;
            setting.DefaultValue = DefaultRetentionDays.ToString();
        });

        context.AddSetting(GroupName, RedisRetryCountKey, setting =>
        {
            setting.Name = "Redis retry count";
            setting.Description = "Số lần retry lỗi Redis tạm thời khi ghi store";
            setting.Type = SettingValueTypes.Number;
            setting.DefaultValue = DefaultRedisRetryCount.ToString();
        });
    }
}
