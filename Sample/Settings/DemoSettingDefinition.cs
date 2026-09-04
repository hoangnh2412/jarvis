using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Validation;

namespace Sample.Settings;

/// <summary>Demo definitions covering every supported setting input type, including IsReadOnly.</summary>
public sealed class DemoSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "Demo";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Demo controls";
            group.Description = "Các kiểu ô nhập hỗ trợ bởi Setting (dùng để kiểm thử UI)";
            group.Order = 0;
        });

        context.AddSetting(GroupName, "Demo.Text", setting =>
        {
            setting.Name = "Text";
            setting.Description = "Ô nhập một dòng.";
            setting.Type = SettingValueTypes.Text;
            setting.Options = SettingTypeOptions.ForText(maxLength: 50);
            setting.DefaultValue = "Hello Setting";
        });

        context.AddSetting(GroupName, "Demo.Email", setting =>
        {
            setting.Name = "Email";
            setting.Description = "Địa chỉ email.";
            setting.Type = SettingValueTypes.Email;
            setting.Options = SettingTypeOptions.ForEmail();
            setting.DefaultValue = "demo@example.com";
        });

        context.AddSetting(GroupName, "Demo.Number", setting =>
        {
            setting.Name = "Number";
            setting.Description = "Số";
            setting.Type = SettingValueTypes.Number;
            setting.Options = SettingTypeOptions.ForNumber(decimals: 2);
            setting.DefaultValue = "1234567.89";
        });

        context.AddSetting(GroupName, "Demo.Textarea", setting =>
        {
            setting.Name = "Textarea";
            setting.Description = "Ô nhiều dòng.";
            setting.Type = SettingValueTypes.Textarea;
            setting.Options = SettingTypeOptions.ForTextarea(rows: 3);
            setting.DefaultValue = "Dòng 1\nDòng 2\nDòng 3";
        });

        context.AddSetting(GroupName, "Demo.Combobox", setting =>
        {
            setting.Name = "Combobox";
            setting.Description = "Chọn một giá trị từ danh sách ngắn.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options = "alpha:Alpha|beta:Beta|gamma:Gamma";
            setting.DefaultValue = "alpha";
        });

        context.AddSetting(GroupName, "Demo.Checkbox", setting =>
        {
            setting.Name = "Checkbox";
            setting.Description = "Bật / tắt.";
            setting.Type = SettingValueTypes.Checkbox;
            setting.DefaultValue = "true";
        });

        context.AddSetting(GroupName, "Demo.Switch", setting =>
        {
            setting.Name = "Switch";
            setting.Description = "Bật / tắt.";
            setting.Type = SettingValueTypes.Switch;
            setting.DefaultValue = "false";
        });

        context.AddSetting(GroupName, "Demo.MultiSelect", setting =>
        {
            setting.Name = "Multi checkbox";
            setting.Description = "Chọn nhiều giá trị bằng checkbox.";
            setting.Type = SettingValueTypes.MultiSelect;
            setting.Options =
                "email:Email|sms:SMS|push:Push notification|inapp:In-app|webhook:Webhook";
            setting.DefaultValue = "email,sms";
        });

        context.AddSetting(GroupName, "Demo.Password", setting =>
        {
            setting.Name = "Password";
            setting.Description = "Mật khẩu.";
            setting.Type = SettingValueTypes.Password;
            setting.IsEncrypted = true;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Demo.Radio", setting =>
        {
            setting.Name = "Radio";
            setting.Description = "Chọn một tùy chọn bằng radio.";
            setting.Type = SettingValueTypes.Radio;
            setting.Options =
                "low:Thấp|medium:Trung bình|high:Cao|critical:Nghiêm trọng|off:Tắt";
            setting.DefaultValue = "medium";
        });

        context.AddSetting(GroupName, "Demo.DateTime", setting =>
        {
            setting.Name = "DateTime";
            setting.Description = "Ngày giờ.";
            setting.Type = SettingValueTypes.DateTime;
            setting.DefaultValue = "2026-07-31T09:30:00";
        });

        context.AddSetting(GroupName, "Demo.Date", setting =>
        {
            setting.Name = "Date";
            setting.Description = "Ngày.";
            setting.Type = SettingValueTypes.Date;
            setting.DefaultValue = "2026-07-31";
        });

        context.AddSetting(GroupName, "Demo.Image", setting =>
        {
            setting.Name = "Image";
            setting.Description = "Upload ảnh.";
            setting.Type = SettingValueTypes.Image;
            setting.Options = SettingTypeOptions.ForImage(
                maxBytes: SettingTypeOptions.DefaultMaxImageBytes,
                mimeTypes: SettingTypeOptions.DefaultImageMimeTypes);
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(GroupName, "Demo.ReadOnly", setting =>
        {
            setting.Name = "Read-only text";
            setting.Description = "IsReadOnly.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = "Giá trị readonly đã persist (demo)";
            setting.IsReadOnly = true;
        });

        context.AddSetting(GroupName, "Demo.ReadOnly.Number", setting =>
        {
            setting.Name = "Read-only number";
            setting.Description = "Chỉ xem.";
            setting.Type = SettingValueTypes.Number;
            setting.Options = SettingTypeOptions.ForNumber(decimals: 0);
            setting.DefaultValue = "1000000";
            setting.IsReadOnly = true;
        });
    }
}
