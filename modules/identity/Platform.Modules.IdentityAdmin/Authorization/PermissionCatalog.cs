using Platform.Permissions.Abstractions;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary>Hằng và luật lọc catalog quyền dùng chung mọi host: claim type, wildcard, full-access.</summary>
public static class PermissionCatalog
{
    public const string ClaimType = "Permission";

    public const string Wildcard = "*";

    public const string SystemAll = "System.All";

    public static bool Grants(IEnumerable<string> granted, string code)
    {
        foreach (var value in granted)
        {
            if (string.Equals(value, Wildcard, StringComparison.Ordinal)
                || string.Equals(value, SystemAll, StringComparison.Ordinal)
                || string.Equals(value, code, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsFullAccess(string code) =>
        string.Equals(code, Wildcard, StringComparison.Ordinal)
        || string.Equals(code, SystemAll, StringComparison.Ordinal);

    /// <summary>Catalog cho UI gán vai trò tầng App — loại quyền chỉ cấp hệ thống (shell, catalog tenant).</summary>
    public static IEnumerable<PermissionDescriptor> ForAppRoleCatalog(IEnumerable<PermissionDescriptor> permissions) =>
        permissions.Where(permission =>
            !string.Equals(permission.Code, PlatformPermissions.ShellAccess, StringComparison.Ordinal)
            && !string.Equals(permission.Code, PlatformPermissions.TenantView, StringComparison.Ordinal)
            && !string.Equals(permission.Code, PlatformPermissions.TenantManage, StringComparison.Ordinal));

    /// <summary>Catalog cho UI gán vai trò tầng Platform.</summary>
    public static IEnumerable<PermissionDescriptor> ForPlatformRoleCatalog(IEnumerable<PermissionDescriptor> permissions) =>
        permissions.Where(permission =>
            string.Equals(permission.Module, "Platform", StringComparison.Ordinal));
}
