using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Constants
{
    public enum UserType
    {
        [Display(Name = "Quản trị viên cấp cao nhất hệ thống")]
        SuperAdmin = 1,

        [Display(Name = "Quản trị viên từng tenant")]
        Admin = 2,

        [Display(Name = "Người dùng được phân quyền sử dụng hệ thống")]
        User = 3,

        [Display(Name = "Người dùng cuối")]
        Guest = 4
    }
}