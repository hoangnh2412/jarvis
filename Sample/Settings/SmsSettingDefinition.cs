using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;

namespace Sample.Settings;

/// <summary>SMS gateway credentials keyed by provider for the host application's SMS sender.</summary>
public sealed class SmsSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "SMS";
    public const string ProviderKey = "SMS.Provider";
    public const string UsernameKey = "SMS.Username";
    public const string PasswordKey = "SMS.Password";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "SMS";
            group.Description = "Cấu hình nhà cung cấp dịch vụ gửi SMS";
            group.Order = 4;
        });

        context.AddSetting(GroupName, ProviderKey, setting =>
        {
            setting.Name = "Provider";
            setting.Description = "Nhà cung cấp dịch vụ SMS.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "twilio:Twilio|vonage:Vonage|aws-sns:Amazon SNS|" +
                "esms:eSMS|speedsms:SpeedSMS|fpt:FPT|custom:Custom";
            setting.DefaultValue = "fpt";
        });

        context.AddSetting(GroupName, UsernameKey, setting =>
        {
            setting.Name = "Username";
            setting.Description =
                "Tên đăng nhập, Account SID, hoặc API key dùng để lấy token / xác thực.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, PasswordKey, setting =>
        {
            setting.Name = "Password";
            setting.Description =
                "Mật khẩu, Auth Token, hoặc API secret tương ứng với Username.";
            setting.Type = SettingValueTypes.Password;
            setting.IsEncrypted = true;
            setting.DefaultValue = string.Empty;
        });
    }
}
