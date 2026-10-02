namespace Platform.Authorization.Abstractions;

using Platform.Authorization.Domain;

/// <summary>
/// Đánh giá một chính sách duy nhất đối với người dùng và tài nguyên.
/// Logics đánh giá chính sách: tất cả các quy tắc trong chính sách được OR'd.
/// Nếu bất kỳ quy tắc nào được đánh giá thành true, chính sách sẽ vượt qua.
/// </summary>
public interface IPolicyEvaluator
{
    /// <summary>
    /// Đánh giá chính sách cho người dùng đối với tài nguyên.
    /// - Tải định nghĩa chính sách (với các quy tắc) từ provider
    /// - Cho mỗi quy tắc: giải quyết operands + so sánh
    /// - Trả về true nếu bất kỳ quy tắc nào vượt qua (semantics OR)
    /// </summary>
    Task<bool> EvaluateAsync(
        Guid userId,
        Policy policy,
        object resource,
        CancellationToken cancellationToken = default);
}
