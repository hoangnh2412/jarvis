namespace Jarvis.Modules.Setting.API.Models;

/// <summary>Body cho Create/Update một setting theo Key.</summary>
public sealed class SettingValueRequest
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>Body lưu cả form theo Group.</summary>
public sealed class SettingGroupSaveRequest
{
    /// <summary>Map setting key → value cho cả form.</summary>
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
