using Platform.Modules.IdentityAdmin.Abstractions;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary><see cref="IPermissionCatalogProvider"/> mặc định trên catalog đã build từ các assembly khai báo.</summary>
public sealed class DefaultPermissionCatalogProvider(IReadOnlyList<PermissionDescriptor> permissions) : IPermissionCatalogProvider
{
    public IReadOnlyList<PermissionCatalogItemDto> GetAppRoleCatalogPermissions() =>
        PermissionCatalog.ForAppRoleCatalog(permissions).Select(MapDescriptor).ToList();

    public IReadOnlyList<PermissionCatalogItemDto> GetPlatformRoleCatalogPermissions() =>
        PermissionCatalog.ForPlatformRoleCatalog(permissions).Select(MapDescriptor).ToList();

    public IReadOnlyList<PermissionCatalogItemDto> GetAllPermissions() =>
        permissions.Select(MapDescriptor).ToList();

    public bool IsFullAccessPermission(string code) => PermissionCatalog.IsFullAccess(code);

    public bool CanManagePolicies(string permissionCode) =>
        IsFullAccessPermission(permissionCode)
        || string.Equals(permissionCode, SystemPermissions.RoleManage, StringComparison.Ordinal);

    private static PermissionCatalogItemDto MapDescriptor(PermissionDescriptor descriptor) =>
        new(
            descriptor.Code,
            descriptor.Module,
            descriptor.Group,
            descriptor.DisplayName,
            descriptor.ModuleOrder,
            descriptor.FieldOrder);
}
