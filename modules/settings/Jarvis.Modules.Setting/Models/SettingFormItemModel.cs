namespace Jarvis.Modules.Setting.Models;

/// <summary>
/// Một ô trên form cấu hình: metadata từ Library + giá trị hiện tại (DB hoặc default).
/// Dùng cho API dựng form UI trong một lần gọi (<c>GetFormAsync</c>).
/// </summary>
public sealed class SettingFormItemModel
{
    /// <summary>Mã nhóm cấu hình.</summary>
    public string Group { get; init; } = string.Empty;

    /// <summary>Mã cấu hình (Key).</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Nhãn hiển thị trên form.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Kiểu control — xem <see cref="SettingValueTypes"/>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Options theo Type (Combobox value:label, hoặc Email/Image/Number/Textarea — xem <c>SettingTypeOptions</c>).</summary>
    public string? Options { get; init; }

    /// <summary>Mô tả / gợi ý hiển thị dưới ô nhập.</summary>
    public string? Description { get; init; }

    /// <summary>Giá trị mặc định từ definition khi chưa có bản ghi DB.</summary>
    public string? DefaultValue { get; init; }

    /// <summary>True nếu UI không được phép sửa ô này.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>True nếu giá trị được mã hóa khi lưu DB (Password/secret).</summary>
    public bool IsEncrypted { get; init; }

    /// <summary>
    /// Giá trị hiện tại để hiển thị/sửa trên form.
    /// Nếu đã có row DB: plaintext đã decrypt; nếu chưa: lấy <see cref="DefaultValue"/>.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>True khi đã có bản ghi trong DB cho Key này; False khi mới dùng default từ Library.</summary>
    public bool IsPersisted { get; init; }
}
