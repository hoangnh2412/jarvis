using Jarvis.DDD.Domain.Entities;

namespace Jarvis.Modules.Setting.EntityFramework.Entities;

/// <summary>
/// Entity Setting mặc định đi kèm package <c>Jarvis.Modules.Setting.EntityFramework</c>.
/// Host có thể dùng trực tiếp, hoặc tự cung cấp kiểu riêng implement <see cref="ISettingEntity"/>.
/// </summary>
public class Setting : BaseEntity<Guid>, ISettingEntity
{
    /// <summary>Tenant sở hữu bản ghi cấu hình.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Mã nhóm logic (ví dụ Email).</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>Mã cấu hình duy nhất trong tenant (ví dụ Email.Host).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Tên hiển thị (snapshot từ definition lúc tạo).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Giá trị lưu trữ (plaintext hoặc ciphertext nếu secret).</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Kiểu giá trị — xem <c>SettingValueTypes</c>.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Danh sách lựa chọn (nếu có), dạng <c>value:label|...</c>.</summary>
    public string? Options { get; set; }

    /// <summary>Mô tả phụ trợ.</summary>
    public string? Description { get; set; }

    /// <summary>True nếu không cho sửa/xóa giá trị.</summary>
    public bool IsReadOnly { get; set; }
}
