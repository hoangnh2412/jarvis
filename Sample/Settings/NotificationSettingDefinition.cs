using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;

namespace Sample.Settings;

/// <summary>Per-tenant toggles for outbound notification channels.</summary>
public sealed class NotificationSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "Notification";
    public const string EnableEmailKey = "Notification.EnableEmail";
    public const string EnableSmsKey = "Notification.EnableSms";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Notification";
            group.Description = "Bật hoặc tắt gửi thông báo theo kênh";
            group.Order = 5;
        });

        context.AddSetting(GroupName, EnableEmailKey, setting =>
        {
            setting.Name = "Enable email";
            setting.Description = "Cho phép gửi thông báo qua email.";
            setting.Type = SettingValueTypes.Radio;
            setting.Options = "true:Bật|false:Tắt";
            setting.DefaultValue = "true";
        });

        context.AddSetting(GroupName, EnableSmsKey, setting =>
        {
            setting.Name = "Enable SMS";
            setting.Description = "Cho phép gửi thông báo qua SMS.";
            setting.Type = SettingValueTypes.Radio;
            setting.Options = "true:Bật|false:Tắt";
            setting.DefaultValue = "false";
        });
    }
}
