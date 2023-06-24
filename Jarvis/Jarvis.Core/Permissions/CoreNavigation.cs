using Infrastructure.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class SystemNavigation : INavigation
    {
        public string Name => "Hệ thống";

        public string Code => "system";

        public string Icon => "fa fa-gears";

        public int Order => 8000;

        public string Url => null;

        public string[] PermissionRequireds => new string[] {  };
    }

    public class TenantInfoNavigation : INavigation {
        public string Name => "Thông tin doanh nghiệp";

        public string Code => "tenant-info";

        public string Icon => "icon-profile";

        public int Order => 8100;

        public string Url => null;

        public string[] PermissionRequireds => new string[] {  };
    }

    public class OrganizationNavigation : INavigation
    {
        public string Name => "Tổ chức";

        public string Code => "organization";

        public string Icon => "";

        public int Order => 8200;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read) };
    }

    public class TenantNavigation : INavigation {
        public string Name => "Quản lý chi nhánh";

        public string Code => "tenants";

        public string Icon => "icon-office";

        public int Order => 8300;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.TenantPolicy.Tenant_Read) };
    }

    public class UserNavigation : INavigation
    {
        public string Name => "Quản lý tài khoản";

        public string Code => "users";

        public string Icon => "icon-users";

        public int Order => 8400;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.UserPolicy.User_Read) };
    }

    public class RoleNavigation : INavigation
    {
        public string Name => "Quản lý vai trò";

        public string Code => "roles";

        public string Icon => "icon-key";

        public int Order => 8500;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.RolePolicy.Role_Read) };
    }

    public class LabelNavigation : INavigation
    {
       public string Name => "Quản lý nhãn";

       public string Code => "labels";

       public string Icon => "";

       public int Order => 8600;

       public string Url => null;

       public string[] PermissionRequireds => new string[] { nameof(CorePolicy.LabelPolicy.Label_Read) };
    }

    public class SettingNavigation : INavigation
    {
        public string Name => "Quản lý tham số hệ thống";

        public string Code => "settings";

        public string Icon => "icon-hammer-wrench";

        public int Order => 8700;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.SettingPolicy.Setting_Read) };
    }

    public class EmailTemplateNavigation : INavigation
    {
       public string Name => "Quản lý mẫu email";

       public string Code => "email-template";

       public string Icon => "";

       public int Order => 8800;

       public string Url => null;

       public string[] PermissionRequireds => new string[] { nameof(CorePolicy.EmailTemplatePolicy.EmailTemplate_Read) };
    }

    public class EmailHistoryNavigation : INavigation
    {
       public string Name => "Lịch sử email";

       public string Code => "email-history";

       public string Icon => "";

       public int Order => 8900;

       public string Url => null;

       public string[] PermissionRequireds => new string[] { nameof(CorePolicy.EmailTemplatePolicy.EmailTemplate_Read) };
    }
}
