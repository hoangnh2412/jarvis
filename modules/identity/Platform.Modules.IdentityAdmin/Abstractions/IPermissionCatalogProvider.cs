namespace Platform.Modules.IdentityAdmin.Abstractions;

public sealed record PermissionCatalogItemDto(
    string Code,
    string Module,
    string Group,
    string DisplayName,
    int ModuleOrder = 0,
    int FieldOrder = 0);

public interface IPermissionCatalogProvider
{
    IReadOnlyList<PermissionCatalogItemDto> GetAppRoleCatalogPermissions();

    IReadOnlyList<PermissionCatalogItemDto> GetPlatformRoleCatalogPermissions();

    IReadOnlyList<PermissionCatalogItemDto> GetAllPermissions();

    bool IsFullAccessPermission(string code);

    bool CanManagePolicies(string permissionCode);
}
