using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Validation;

namespace Sample.Settings;

/// <summary>SMTP email settings used by the host application's email sender.</summary>
public sealed class EmailSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "Email";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Email";
            group.Description = "Cấu hình máy chủ gửi email SMTP";
            group.Order = 1;
        });

        context.AddSetting(GroupName, "Email.FromAddress", setting =>
        {
            setting.Name = "From Address";
            setting.Description = "Địa chỉ email của người gửi.";
            setting.Type = SettingValueTypes.Email;
            setting.Options = SettingTypeOptions.ForEmail(); // regex:default
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Email.DisplayName", setting =>
        {
            setting.Name = "Display Name";
            setting.Description = "Tên hiển thị của người gửi.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Email.Host", setting =>
        {
            setting.Name = "Host";
            setting.Description = "Tên máy chủ SMTP.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Email.Port", setting =>
        {
            setting.Name = "Port";
            setting.Description = "Cổng thường dùng: 587 (STARTTLS).";
            setting.Type = SettingValueTypes.Number;
            setting.Options = SettingTypeOptions.ForNumber(decimals: 0);
            setting.DefaultValue = "587";
        });

        context.AddSetting(GroupName, "Email.Username", setting =>
        {
            setting.Name = "Username";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Email.Password", setting =>
        {
            setting.Name = "Password";
            setting.Type = SettingValueTypes.Password;
            setting.IsEncrypted = true;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Email.EnableSsl", setting =>
        {
            setting.Name = "Enable SSL";
            setting.Description = "Bật TLS/SSL cho kết nối SMTP.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options = "true:Yes|false:No";
            setting.DefaultValue = "true";
        });
    }
}
