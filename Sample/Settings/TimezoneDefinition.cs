using Jarvis.Modules.Setting;
using Jarvis.Modules.Setting.Definitions;

namespace Sample.Settings;

public sealed class TimezoneDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "Timezone";
    public const string DefaultTimezoneKey = "Timezone.Default";

    private static readonly (string IanaId, string WindowsId)[] Timezones =
    {
        ("Pacific/Honolulu", "Hawaiian Standard Time"),
        ("America/Los_Angeles", "Pacific Standard Time"),
        ("America/Denver", "Mountain Standard Time"),
        ("America/Chicago", "Central Standard Time"),
        ("America/New_York", "Eastern Standard Time"),
        ("America/Sao_Paulo", "E. South America Standard Time"),
        ("Etc/UTC", "UTC"),
        ("Europe/London", "GMT Standard Time"),
        ("Europe/Paris", "Romance Standard Time"),
        ("Europe/Athens", "GTB Standard Time"),
        ("Asia/Dubai", "Arabian Standard Time"),
        ("Asia/Kolkata", "India Standard Time"),
        ("Asia/Bangkok", "SE Asia Standard Time"),
        ("Asia/Ho_Chi_Minh", "SE Asia Standard Time"),
        ("Asia/Shanghai", "China Standard Time"),
        ("Asia/Tokyo", "Tokyo Standard Time"),
        ("Australia/Sydney", "AUS Eastern Standard Time"),
        ("Pacific/Auckland", "New Zealand Standard Time"),
    };

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, group =>
        {
            group.DisplayName = "Timezone";
            group.Description = "Múi giờ mặc định của tenant";
            group.Order = 2;
        });

        context.AddSetting(GroupName, DefaultTimezoneKey, setting =>
        {
            setting.Name = "Default timezone";
            setting.Description = "Múi giờ dùng để hiển thị giá trị ngày và giờ.";
            setting.Type = SettingValueTypes.Combobox;
            setting.Options = BuildOptions();
            setting.DefaultValue = "Asia/Ho_Chi_Minh";
        });
    }

    private static string BuildOptions() =>
        string.Join("|", Timezones.Select(timezone =>
            $"{timezone.IanaId}:{FormatLabel(timezone.IanaId, timezone.WindowsId)}"));

    private static string FormatLabel(string ianaId, string windowsId)
    {
        var systemId = OperatingSystem.IsWindows() ? windowsId : ianaId;
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(systemId);
        var offset = timezone.GetUtcOffset(DateTimeOffset.UtcNow);
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var absoluteOffset = offset.Duration();
        return $"(UTC{sign}{absoluteOffset:hh\\:mm}) {ianaId}";
    }
}