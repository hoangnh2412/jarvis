using Infrastructure.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class SystemNavigation : INavigation
    {
        public string Name => "Hệ thống";

        public string Code => "system";

        public string Icon => "fa fa-gears";

        public int Order => 2000;

        public string Url => null;

        public string[] PermissionRequireds => new string[] {  };
    }

    public class TenantInfonavigation : INavigation {
        public string Name => "Thông tin doanh nghiệp";

        public string Code => "tenant-info";

        public string Icon => "fa fa-info";

        public int Order => 2100;

        public string Url => null;

        public string[] PermissionRequireds => new string[] {  };
    }

    public class OrganizationNavigation : INavigation
    {
        public string Name => "Quản lý tổ chức";

        public string Code => "organizations";

        public string Icon => "fa fa-users";

        public int Order => 2200;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.OrganizationPolicy.Organization_Read) };
    }

    public class Tenantnavigation : INavigation {
        public string Name => "Quản lý chi nhánh";

        public string Code => "tenants";

        public string Icon => "fa fa-tree";

        public int Order => 2300;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.TenantPolicy.Tenant_Read) };
    }

    public class UserNavigation : INavigation
    {
        public string Name => "Quản lý tài khoản";

        public string Code => "users";

        public string Icon => "fa fa-user-secret";

        public int Order => 2400;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.UserPolicy.User_Read) };
    }

    public class RoleNavigation : INavigation
    {
        public string Name => "Quản lý quyền";

        public string Code => "roles";

        public string Icon => "fa fa-key";

        public int Order => 2500;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.RolePolicy.Role_Read) };
    }

    public class LabelNavigation : INavigation
    {
        public string Name => "Quản lý nhãn";

        public string Code => "labels";

        public string Icon => "fa fa-tag";

        public int Order => 2600;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.LabelPolicy.Label_Read) };
    }

    public class SettingNavigation : INavigation
    {
        public string Name => "Quản lý tham số hệ thống";

        public string Code => "settings";

        public string Icon => "fa fa-cog";

        public int Order => 2700;

        public string Url => null;

        public string[] PermissionRequireds => new string[] { nameof(CorePolicy.SettingPolicy.Setting_Read) };
    }
}
