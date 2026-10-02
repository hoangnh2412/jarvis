namespace Platform.Authorization.Abstractions;

/// <summary>
/// Role Governance (ADR C5, §5.4): an actor may only act on roles/users whose level is
/// strictly lower than the actor's highest RoleLevel. Tenant/Org scope is not checked here.
/// </summary>
/// <remarks>
/// Quản trị vai trò: một người dùng chỉ có thể hành động trên các vai trò/người dùng có cấp độ
/// THẤP HƠN cấp độ vai trò cao nhất của họ. Scope của Tenant/Org không được kiểm tra ở đây.
/// </remarks>
public interface IRoleGovernanceValidator
{
    /// <summary>Highest RoleLevel among the user's roles; 0 when the user has no role.</summary>
    /// <remarks>Lấy cấp độ vai trò cao nhất của người dùng; trả về 0 nếu người dùng không có vai trò nào.</remarks>
    Task<int> GetMaxRoleLevelAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Actor may modify the role (permissions/policies) only if actor level &gt; role level. Unknown role → false.</summary>
    /// <remarks>Kiểm tra người dùng có quyền sửa đổi vai trò (quyền hạn/chính sách) không. Chỉ được phép nếu cấp độ của họ CAO HƠN cấp độ vai trò đó. Nếu vai trò không tồn tại → trả về false.</remarks>
    Task<bool> CanManageRoleAsync(Guid actorUserId, Guid targetRoleId, CancellationToken cancellationToken = default);

    /// <summary>Actor may assign the role to a user only if actor level &gt; role level.</summary>
    /// <remarks>Kiểm tra người dùng có quyền gán vai trò cho người khác không. Chỉ được phép nếu cấp độ của họ CAO HƠN cấp độ vai trò đó.</remarks>
    Task<bool> CanAssignRoleAsync(Guid actorUserId, Guid targetRoleId, CancellationToken cancellationToken = default);

    /// <summary>Actor may create a role with the given level only if actor level &gt; that level.</summary>
    /// <remarks>Kiểm tra người dùng có quyền tạo vai trò mới với cấp độ nhất định không. Chỉ được phép nếu cấp độ của họ CAO HƠN cấp độ vai trò mà họ muốn tạo.</remarks>
    Task<bool> CanCreateRoleWithLevelAsync(Guid actorUserId, int roleLevel, CancellationToken cancellationToken = default);

    /// <summary>Actor may lock/unlock/delete a user only if actor level &gt; the user's highest role level.</summary>
    /// <remarks>Kiểm tra người dùng có quyền quản lý người dùng khác (khóa/mở khóa/xóa) không. Chỉ được phép nếu cấp độ cao nhất của họ CAO HƠN cấp độ cao nhất của người dùng đó.</remarks>
    Task<bool> CanManageUserAsync(Guid actorUserId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <remarks>Kiểm tra một vai trò có phải là vai trò hệ thống (không được xóa/sửa) không.</remarks>
    Task<bool> IsSystemRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
