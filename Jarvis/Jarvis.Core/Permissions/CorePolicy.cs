using Infrastructure.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Permissions
{
    [Display(Name = "Quản lý hệ thống", Order = 999)]
    public class CorePolicy
    {
        [Display(Name = "Quản lý người dùng")]
        public class UserPolicy : IPolicy
        {
            public static string User_Read = "Danh sách người dùng";
            public static string User_Create = "Tạo người dùng";
            public static string User_Update = "Sửa người dùng";
            public static string User_Delete = "Xóa người dùng";
            public static string User_Lock = "Khóa người dùng";
            public static string User_Reset_Password = "Đổi mật khẩu tài khoản";
        }

        [Display(Name = "Quản lý vai trò")]
        public class RolePolicy : IPolicy
        {
            public static string Role_Read = "Danh sách vai trò";
            public static string Role_Create = "Tạo vai trò";
            public static string Role_Update = "Sửa vai trò";
            public static string Role_Delete = "Xóa vai trò";
        }

        [Display(Name = "Quản lý tenant")]
        public class TenantPolicy : IPolicy
        {
            public static string Tenant_Read = "Danh sách tenant";
            public static string Tenant_Create = "Tạo tenant";
            public static string Tenant_Update = "Sửa tenant";
            public static string Tenant_Delete = "Xóa tenant";
        }

        [Display(Name = "Quản lý nhãn")]
        public class LabelPolicy : IPolicy
        {
            public static string Label_Read = "Danh sách nhãn";
            public static string Label_Create = "Tạo nhãn";
            public static string Label_Update = "Sửa nhãn";
            public static string Label_Delete = "Xóa nhãn";
        }

        [Display(Name = "Quản lý tham số")]
        public class SettingPolicy : IPolicy
        {
            public static string Setting_Read = "Danh sách tham số";
            public static string Setting_Update = "Sửa tham số";
            public static string Setting_Import = "Import tham số";
            public static string Setting_Export = "Export tham số";
        }

        [Display(Name = "Quản lý phòng ban")]
        public class OrganizationPolicy : IPolicy
        {
            public static string OrganizationUnit_Read = "Danh sách phòng ban";
            public static string OrganizationUnit_Create = "Tạo phòng ban";
            public static string OrganizationUnit_Update = "Sửa phòng ban";
            public static string OrganizationUnit_Delete = "Xóa phòng ban";
            public static string OrganizationUnit_Users = "Thành viên trong phòng ban";
            public static string OrganizationUnit_Roles = "Vai trò trong phòng ban";
        }

        [Display(Name = "Quản lý mẫu email")]
        public class EmailTemplatePolicy : IPolicy
        {
            public static string EmailTemplate_Read = "Danh sách mẫu email";
            public static string EmailTemplate_Create = "Tạo mẫu email";
            public static string EmailTemplate_Update = "Sửa mẫu email";
            public static string EmailTemplate_Delete = "Xóa mẫu email";
        }

        [Display(Name = "Quản lý lịch sử email")]
        public class EmailHistoryPolicy : IPolicy
        {
            public static string EmailHistory_Read = "Lịch sử email";
            public static string EmailHistory_Update = "Gửi lại mail";
            public static string EmailHistory_Delete = "Xoá lịch sử mail";
        }
    }
}
