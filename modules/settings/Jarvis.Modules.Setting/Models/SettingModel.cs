namespace Jarvis.Modules.Setting.Models;

/// <summary>
/// DTO trả về từ <see cref="Services.ISettingManager"/>.
/// <c>Value</c> luôn là plaintext (đã giải mã nếu là secret) để caller nghiệp vụ dùng trực tiếp.
/// </summary>
public sealed class SettingModel
{
    /// <summary>Id bản ghi (UUID v7 khi tạo mới).</summary>
    public Guid Id { get; init; }

    /// <summary>Tenant sở hữu giá trị cấu hình.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Mã nhóm logic (ví dụ Email). Metadata hiển thị nhóm nằm ở Library, không lưu DB.</summary>
    public string Group { get; init; } = string.Empty;

    /// <summary>Mã cấu hình duy nhất trong tenant (ví dụ Email.Host).</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Tên hiển thị (snapshot từ definition lúc tạo/cập nhật).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Giá trị plaintext. Secret đã được decrypt trước khi trả về.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Kiểu giá trị — xem <see cref="SettingValueTypes"/>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Danh sách lựa chọn (nếu có), dạng <c>value:label|...</c>.</summary>
    public string? Options { get; init; }

    /// <summary>Mô tả phụ trợ cho UI/người dùng.</summary>
    public string? Description { get; init; }

    /// <summary>True nếu không cho sửa/xóa giá trị.</summary>
    public bool IsReadOnly { get; init; }
}
