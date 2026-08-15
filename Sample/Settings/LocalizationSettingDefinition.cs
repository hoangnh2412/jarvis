using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;

namespace Sample.Settings;

public sealed class LocalizationSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "Localization";
    public const string LanguageKey = "Localization.Language";
    public const string DateFormatKey = "Localization.DateFormat";
    public const string TimeFormatKey = "Localization.TimeFormat";
    public const string FirstDayOfWeekKey = "Localization.FirstDayOfWeek";
    public const string CurrencyKey = "Localization.Currency";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Localization";
            group.Description = "Tùy chọn hiển thị ngôn ngữ và khu vực";
            group.Order = 3;
        });

        context.AddSetting(GroupName, LanguageKey, setting =>
        {
            setting.Name = "Language";
            setting.Description = "Ngôn ngữ sử dụng trên giao diện người dùng.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "vi-VN:Tiếng Việt|en-US:English (United States)|en-GB:English (United Kingdom)|" +
                "ja-JP:日本語|ko-KR:한국어|zh-CN:简体中文|fr-FR:Français|de-DE:Deutsch";
            setting.DefaultValue = "vi-VN";
        });

        context.AddSetting(GroupName, DateFormatKey, setting =>
        {
            setting.Name = "Date format";
            setting.Description = "Định dạng dùng để hiển thị ngày.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "dd/MM/yyyy:DD/MM/YYYY|MM/dd/yyyy:MM/DD/YYYY|" +
                "yyyy-MM-dd:YYYY-MM-DD|dd-MM-yyyy:DD-MM-YYYY";
            setting.DefaultValue = "dd/MM/yyyy";
        });

        context.AddSetting(GroupName, TimeFormatKey, setting =>
        {
            setting.Name = "Time format";
            setting.Description = "Định dạng dùng để hiển thị thời gian.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "HH%3Amm:24-hour (HH:mm)|HH%3Amm%3Ass:24-hour with seconds (HH:mm:ss)|" +
                "hh%3Amm%20tt:12-hour (hh:mm AM/PM)|" +
                "hh%3Amm%3Ass%20tt:12-hour with seconds (hh:mm:ss AM/PM)";
            setting.DefaultValue = "HH:mm";
        });

        context.AddSetting(GroupName, FirstDayOfWeekKey, setting =>
        {
            setting.Name = "First day of week";
            setting.Description = "Ngày đầu tiên của tuần hiển thị trên lịch.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "Monday:Monday|Tuesday:Tuesday|Wednesday:Wednesday|Thursday:Thursday|" +
                "Friday:Friday|Saturday:Saturday|Sunday:Sunday";
            setting.DefaultValue = "Monday";
        });

        context.AddSetting(GroupName, CurrencyKey, setting =>
        {
            setting.Name = "Currency";
            setting.Description = "Đơn vị tiền tệ mặc định dùng để hiển thị giá trị tiền.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options =
                "VND:Vietnamese đồng (VND)|USD:US dollar (USD)|EUR:Euro (EUR)|" +
                "GBP:Pound sterling (GBP)|JPY:Japanese yen (JPY)|KRW:South Korean won (KRW)|" +
                "CNY:Chinese yuan (CNY)|AUD:Australian dollar (AUD)|CAD:Canadian dollar (CAD)|" +
                "SGD:Singapore dollar (SGD)";
            setting.DefaultValue = "VND";
        });
    }
}
