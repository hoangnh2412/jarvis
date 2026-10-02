namespace Platform.Authorization.Domain;

/// <summary>
/// Gốc tập hợp chính sách cho ABAC (Kiểm soát truy cập dựa trên thuộc tính).
/// Các chính sách hệ thống (P_SAME_ORG, P_OWNER_ONLY, v.v.) được hạt giống và không thay đổi.
/// Các chính sách tùy chỉnh có thể được phép để tùy chỉnh sản phẩm (giai đoạn 4b+).
/// </summary>
public sealed class Policy
{
    public Guid Id { get; set; }

    /// <summary>
    /// Định danh mã duy nhất (ví dụ: "P_SAME_ORG", "P_APPROVE_LIMIT").
    /// Được sử dụng làm khóa để giải quyết thời gian chạy.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị (ví dụ: "Cùng tổ chức").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tùy chọn: loại tài nguyên mà chính sách này áp dụng cho (ví dụ: "Expense", "Project").
    /// Null có nghĩa là chính sách là chung (áp dụng cho bất kỳ tài nguyên nào).
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Tùy chọn: hành động mà chính sách này áp dụng cho (ví dụ: "View", "Approve").
    /// Null có nghĩa là chính sách áp dụng cho bất kỳ hành động nào.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Chính sách hệ thống không thể được sửa đổi hoặc xóa; chỉ các chính sách tùy chỉnh có thể được quản lý.
    /// </summary>
    public bool IsSystemPolicy { get; set; }

    /// <summary>
    /// Bộ sưu tập các quy tắc xác định logic đánh giá chính sách.
    /// Cần ít nhất một quy tắc cho chính sách hợp lệ.
    /// </summary>
    public ICollection<PolicyRule> Rules { get; set; } = new List<PolicyRule>();

    /// <summary>
    /// Dấu thời gian để kiểm toán.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
