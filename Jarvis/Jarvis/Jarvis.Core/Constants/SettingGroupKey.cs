using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Constants
{
    public enum SettingGroupKey
    {
        [Display(Name = "Thông tin doanh nghiệp")]
        ThongTinDoanhNghiep = 1,

        [Display(Name = "Đăng nhập")]
        Login = 2,
    }
}
