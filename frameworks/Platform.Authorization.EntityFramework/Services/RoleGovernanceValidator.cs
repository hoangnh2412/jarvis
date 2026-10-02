namespace Platform.Authorization.EntityFramework.Services;

using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;

/// <summary>
/// Triển khai EF của <see cref="IRoleGovernanceValidator"/> trên kho lưu trữ Danh tính được chia sẻ.
/// </summary>
public sealed class RoleGovernanceValidator : IRoleGovernanceValidator
{
    private readonly IdentityDbContextBase _context;

    public RoleGovernanceValidator(IdentityDbContextBase context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Cấp độ vai trò cao nhất giữa các vai trò của người dùng; 0 khi người dùng không có vai trò.
    /// </summary>
    public async Task<int> GetMaxRoleLevelAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var maxLevel = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId
                select (int?)r.RoleLevel)
            .MaxAsync(cancellationToken);

        return maxLevel ?? 0;
    }

    /// <summary>
    /// Người dùng chỉ có thể sửa đổi vai trò (quyền hạn/chính sách của nó) nếu cấp độ của họ CAO HƠN cấp độ vai trò.
    /// Vai trò không rõ → false.
    /// </summary>
    public async Task<bool> CanManageRoleAsync(
        Guid actorUserId,
        Guid targetRoleId,
        CancellationToken cancellationToken = default)
    {
        var targetLevel = await _context.Roles
            .Where(r => r.Id == targetRoleId)
            .Select(r => (int?)r.RoleLevel)
            .FirstOrDefaultAsync(cancellationToken);

        if (targetLevel is null)
        {
            return false;
        }

        return await GetMaxRoleLevelAsync(actorUserId, cancellationToken) > targetLevel.Value;
    }

    /// <summary>
    /// Người dùng chỉ có thể gán vai trò cho người dùng nếu cấp độ của họ CAO HƠN cấp độ vai trò.
    /// </summary>
    public Task<bool> CanAssignRoleAsync(
        Guid actorUserId,
        Guid targetRoleId,
        CancellationToken cancellationToken = default) =>
        CanManageRoleAsync(actorUserId, targetRoleId, cancellationToken);

    /// <summary>
    /// Actor may create a role with the given level only if actor level &gt; that level.
    /// </summary>
    public async Task<bool> CanCreateRoleWithLevelAsync(
        Guid actorUserId,
        int roleLevel,
        CancellationToken cancellationToken = default) =>
        await GetMaxRoleLevelAsync(actorUserId, cancellationToken) > roleLevel;

    /// <summary>
    /// Actor may lock/unlock/delete a user only if actor level &gt; the user's highest role level.
    /// </summary>
    public async Task<bool> CanManageUserAsync(
        Guid actorUserId,
        Guid targetUserId,
        CancellationToken cancellationToken = default) =>
        await GetMaxRoleLevelAsync(actorUserId, cancellationToken)
        > await GetMaxRoleLevelAsync(targetUserId, cancellationToken);

    public Task<bool> IsSystemRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        _context.Roles.AnyAsync(r => r.Id == roleId && r.IsSystemRole, cancellationToken);
}
