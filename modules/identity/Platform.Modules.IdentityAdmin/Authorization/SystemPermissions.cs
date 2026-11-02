using System.ComponentModel.DataAnnotations;

namespace Platform.Modules.IdentityAdmin.Authorization;

[Display(Name = "System", Description = "Hệ thống", Order = 4)]
public sealed class SystemPermissions : IPermissionDefinition
{
    [Display(Description = "Quản lý người dùng", Order = 1)]
    public const string UserManage = "System.User.Manage";

    [Display(Description = "Quản lý vai trò", Order = 2)]
    public const string RoleManage = "System.Role.Manage";

    [Display(Description = "Xem thông tin doanh nghiệp", Order = 3)]
    public const string CompanyView = "System.Company.View";

    [Display(Description = "Quản lý thông tin doanh nghiệp", Order = 4)]
    public const string CompanyManage = "System.Company.Manage";

    [Display(Description = "Xem đơn vị", Order = 5)]
    public const string UnitView = "System.Unit.View";

    [Display(Description = "Quản lý đơn vị", Order = 6)]
    public const string UnitManage = "System.Unit.Manage";

    [Display(Description = "Xem phạm vi tổ chức (catalog Org)", Order = 7)]
    public const string OrgView = "Org.View";

    [Display(Description = "Quản lý phạm vi tổ chức (catalog Org)", Order = 8)]
    public const string OrgManage = "Org.Manage";

    [Display(Description = "Xem cấu hình email SMTP", Order = 9)]
    public const string EmailView = "System.Email.View";

    [Display(Description = "Cập nhật cấu hình email SMTP", Order = 10)]
    public const string EmailManage = "System.Email.Manage";
}
