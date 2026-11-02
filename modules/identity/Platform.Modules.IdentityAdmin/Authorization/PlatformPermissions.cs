using System.ComponentModel.DataAnnotations;

namespace Platform.Modules.IdentityAdmin.Authorization;

[Display(Name = "Platform", Description = "Shell", Order = 0)]
public sealed class PlatformPermissions : IPermissionDefinition
{
    [Display(Description = "Vào ứng dụng", Order = 1)]
    public const string ShellAccess = "Platform.Shell.Access";

    [Display(Description = "Xem catalog tenant", Order = 2)]
    public const string TenantView = "Platform.Tenant.View";

    [Display(Description = "Quản lý catalog tenant", Order = 3)]
    public const string TenantManage = "Platform.Tenant.Manage";
}
