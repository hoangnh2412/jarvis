namespace Jarvis.Modules.Setting.EntityFramework.Configuration;

/// <summary>
/// Tuỳ chọn ghi đè tên bảng / schema khi áp dụng mapping EF mặc định cho Setting.
/// </summary>
public sealed class SettingModelBuilderConfigurationOptions
{
    /// <summary>Tên bảng lưu Setting. Mặc định <c>Setting</c>.</summary>
    public string TableName { get; set; } = "Setting";

    /// <summary>Schema DB (tuỳ chọn). Null/rỗng = không chỉ định schema.</summary>
    public string? Schema { get; set; }
}
