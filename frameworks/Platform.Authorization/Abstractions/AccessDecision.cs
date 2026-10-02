namespace Platform.Authorization.Abstractions;

/// <summary>
/// Kết quả của đánh giá chính sách ABAC.
/// </summary>
public sealed record AccessDecision
{
    /// <summary>
    /// Liệu đánh giá chính sách có vượt qua hay không.
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Lý do tùy chọn cho việc từ chối (để ghi nhật ký/kiểm toán).
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Tên của chính sách vượt qua (cho dấu vết kiểm toán).
    /// </summary>
    public string? PolicyCode { get; init; }

    public static AccessDecision Allow(string? policyCode = null) =>
        new() { IsAllowed = true, PolicyCode = policyCode };

    public static AccessDecision Deny(string reason, string? policyCode = null) =>
        new() { IsAllowed = false, Reason = reason, PolicyCode = policyCode };
}
