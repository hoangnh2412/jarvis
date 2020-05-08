using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Constants
{
    public enum SettingKey
    {
        [Display(Name = "Thông tin khác")]
        ThongTinDoanhNghiep_Khac = 1,

        [Display(Name = "Hình thức đăng nhập")]
        Login_Type = 2,

        [Display(Name = "Tên thư mục lưu file")]
        FilePathOption = 3,
    }
}
