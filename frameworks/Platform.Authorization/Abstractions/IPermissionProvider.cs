namespace Platform.Authorization.Abstractions;

/// <summary>
/// Cung cấp quyền hạn cho người dùng dựa trên các vai trò được gán.
/// Truy vấn AspNetUserRoles → AspNetRoleClaims nơi ClaimType = "Permission".
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Lấy tất cả các quyền hạn được cấp cho người dùng thông qua các vai trò được gán.
    /// Trả về tập hợp các chuỗi quyền hạn (ví dụ: "Expense.View", "Expense.Approve", "System.All").
    /// </summary>
    Task<ISet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy các quyền hạn được nhóm theo vai trò của người dùng (roleId → permissions), để sử dụng đường dẫn ABAC theo vai trò.
    /// Các vai trò không có yêu cầu quyền hạn nào sẽ bị bỏ qua.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ISet<string>>> GetPermissionsByRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem người dùng có quyền hạn đó hay không, trực tiếp hoặc thông qua wildcard ("*" / "System.All").
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
}
