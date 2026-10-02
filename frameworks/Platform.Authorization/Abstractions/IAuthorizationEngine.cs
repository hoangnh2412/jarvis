namespace Platform.Authorization.Abstractions;

/// <summary>
/// Công cụ ủy quyền chính điều phối kiểm tra RBAC + ABAC.
/// </summary>
public interface IAuthorizationEngine
{
    /// <summary>
    /// Kiểm tra xem người dùng có quyền hạn (kiểm tra RBAC chỉ).
    /// Trả về true nếu quyền hạn tồn tại trong các vai trò được gán,
    /// hoặc nếu người dùng có quyền hạn wildcard (Permission = "*").
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh giá các chính sách ABAC cho người dùng và tài nguyên.
    /// - Nếu người dùng có quyền hạn wildcard (Permission = "*"), trả về true ngay lập tức (Admin bypass).
    /// - Ngược lại, chỉ xem xét các vai trò cấp quyền <paramref name="permission"/>.
    /// - Đánh giá đường dẫn theo vai trò: trong một vai trò tất cả các chính sách phải vượt qua (AND);
    ///   kết quả = hợp nhất trên các vai trò (bất kỳ đường dẫn vai trò nào vượt qua → cho phép).
    /// </summary>
    Task<AccessDecision> EvaluatePoliciesAsync(
        Guid userId,
        string permission,
        object resource,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra ủy quyền (quyền hạn + ABAC) trong một lệnh gọi duy nhất.
    /// </summary>
    Task<bool> AuthorizeAsync(
        Guid userId,
        string permission,
        object? resource = null,
        CancellationToken cancellationToken = default);
}
