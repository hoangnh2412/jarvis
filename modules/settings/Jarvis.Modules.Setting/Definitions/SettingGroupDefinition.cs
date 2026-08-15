namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Metadata code-first của một nhóm cấu hình trong Library.
/// Chỉ tồn tại trong bộ nhớ — không có bảng/cột lưu Group metadata trên DB.
/// </summary>
public sealed class SettingGroupDefinition
{
    /// <summary>
    /// Khởi tạo nhóm với mã bắt buộc.
    /// </summary>
    /// <param name="name">Mã nhóm (ví dụ Email). Khớp với <c>ISettingEntity.Group</c>.</param>
    public SettingGroupDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>Mã nhóm logic dùng để gắn Key và lọc theo group.</summary>
    public string Name { get; }

    /// <summary>Tên hiển thị trên menu/tab UI. Nếu để trống khi đăng ký, Registry gán bằng Name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Mô tả ngắn về nhóm (tuỳ chọn).</summary>
    public string? Description { get; set; }

    /// <summary>Thứ tự sắp xếp khi liệt kê nhóm (số nhỏ hơn hiện trước).</summary>
    public int Order { get; set; }
}
