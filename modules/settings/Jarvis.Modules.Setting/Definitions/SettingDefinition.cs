namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Metadata code-first của một Key cấu hình trong Library.
/// Không lưu xuống DB; khi runtime tạo row thì Manager copy một phần metadata vào <c>ISettingEntity</c>.
/// </summary>
public sealed class SettingDefinition
{
    /// <summary>
    /// Khởi tạo definition với Group và Key bắt buộc.
    /// </summary>
    /// <param name="group">Mã nhóm sở hữu Key.</param>
    /// <param name="key">Mã cấu hình duy nhất trong Library / trong mỗi tenant.</param>
    public SettingDefinition(string group, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Group = group;
        Key = key;
    }

    /// <summary>Mã nhóm logic (ví dụ Email).</summary>
    public string Group { get; }

    /// <summary>Mã cấu hình duy nhất trong tenant (ví dụ Email.Host).</summary>
    public string Key { get; }

    /// <summary>Nhãn hiển thị trên UI. Nếu để trống khi đăng ký, Registry gán bằng Key.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kiểu giá trị / control UI.
    /// Mặc định <see cref="SettingValueTypes.Text"/>. Xem thêm <see cref="SettingValueTypes"/>.
    /// </summary>
    public string Type { get; set; } = SettingValueTypes.Text;

    /// <summary>
    /// Cấu hình theo Type — hai ngữ nghĩa (không trộn):
    /// <list type="bullet">
    /// <item>
    /// <b>Choice</b> — Combobox / Radio / MultiSelect:
    /// danh sách giá trị chọn <c>value:label|value:label|...</c>
    /// </item>
    /// <item>
    /// <b>Constraint</b> — Text / Textarea / Number / Email / Image:
    /// điều kiện giá trị <c>key:value|...</c>
    /// (Email: <c>regex:default</c> hoặc pattern riêng; Number: <c>decimals</c>;
    /// Text: <c>maxLength</c>; Image: <c>maxBytes</c>/<c>mimeTypes</c>; Textarea: <c>rows</c>…)
    /// </item>
    /// </list>
    /// Dùng <see cref="Validation.SettingTypeOptions"/> để build/parse constraint.
    /// </summary>
    public string? Options { get; set; }

    /// <summary>Mô tả nghiệp vụ hiển thị kèm ô nhập.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá trị mặc định khi tạo row lần đầu (Create / GetOrCreate) hoặc khi form chưa có bản ghi.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// True: không cho Update/Delete giá trị.
    /// Snapshot cũng được ghi vào entity khi tạo row.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// True: Value được mã hóa AES-GCM khi ghi DB/cache.
    /// Với Type = Password, Registry tự bật cờ này nếu chưa set.
    /// </summary>
    public bool IsEncrypted { get; set; }
}
