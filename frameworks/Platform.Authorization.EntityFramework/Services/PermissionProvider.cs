namespace Platform.Authorization.EntityFramework.Services;

using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;

/// <summary>
/// Cung cấp quyền hạn cho người dùng dựa trên các vai trò được gán.
/// Truy vấn UserRoles → RoleClaims nơi ClaimType = "Permission".
/// </summary>
public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IdentityDbContextBase _context;

    public PermissionProvider(IdentityDbContextBase context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ISet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Truy vấn các vai trò của người dùng
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (!roleIds.Any())
        {
            return new HashSet<string>();
        }

        // Truy vấn các quyền hạn (RoleClaims nơi ClaimType = "Permission") cho tất cả các vai trò của người dùng
        var permissions = await _context.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "Permission")
            .Select(rc => rc.ClaimValue ?? string.Empty)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<string>(permissions);
    }

    public async Task<IReadOnlyDictionary<Guid, ISet<string>>> GetPermissionsByRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await (
                from ur in _context.UserRoles
                join rc in _context.RoleClaims on ur.RoleId equals rc.RoleId
                where ur.UserId == userId && rc.ClaimType == "Permission"
                select new { rc.RoleId, rc.ClaimValue })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.RoleId)
            .ToDictionary(
                g => g.Key,
                g => (ISet<string>)g.Select(r => r.ClaimValue ?? string.Empty).ToHashSet());
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, cancellationToken);
        return PermissionMatcher.Grants(permissions, permission);
    }
}
