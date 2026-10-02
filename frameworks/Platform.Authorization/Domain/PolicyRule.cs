namespace Platform.Authorization.Domain;

/// <summary>
/// Quy tắc chính sách đại diện cho một điều kiện duy nhất trong đánh giá ABAC.
/// Ví dụ: Resource.OrgId == User.OrgId
/// Nhiều quy tắc trong cùng một chính sách được OR'd (semantics hợp nhất).
/// </summary>
public sealed class PolicyRule
{
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa ngoại cho chính sách cha.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Operand bên trái (ví dụ: Resource.OrgId, Resource.Amount).
    /// Danh sách trắng được thực thi tại thời điểm tạo/xác thực.
    /// Định dạng: Source.Attribute nơi Source là Resource hoặc User
    /// </summary>
    public string LeftOperand { get; set; } = string.Empty;

    /// <summary>
    /// Toán tử so sánh (ví dụ: "==", "!=", "&lt;", "&lt;=", "&gt;", "&gt;=", "IN").
    /// Danh sách trắng được thực thi; không có toán tử phức tạp.
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Operand bên phải (ví dụ: User.OrgId, giá trị hằng số 2000000).
    /// Có thể là thuộc tính User/Resource khác hoặc hằng số.
    /// </summary>
    public string RightOperand { get; set; } = string.Empty;

    /// <summary>
    /// Thứ tự trong cùng một chính sách (1, 2, 3, ...).
    /// Đối với các chính sách nhiều quy tắc với semantics OR, xác định thứ tự đánh giá.
    /// Khóa kép: (PolicyId, Order) phải duy nhất.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Dấu thời gian để kiểm toán.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thuộc tính điều hướng cho EF.
    /// </summary>
    public Policy? Policy { get; set; }
}
