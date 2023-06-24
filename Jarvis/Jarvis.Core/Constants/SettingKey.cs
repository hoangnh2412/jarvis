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

        #region SMTP
        [Display(Name = "SMTP From")]
        Smtp_From,

        [Display(Name = "SMTP From Name")]
        Smtp_FromName,

        [Display(Name = "SMTP host")]
        Smtp_Host,

        [Display(Name = "SMTP socket")]
        Smtp_Socket,

        [Display(Name = "SMTP port")]
        Smtp_Port,

        [Display(Name = "SMTP Authentication")]
        Smtp_Authentication,

        [Display(Name = "SMTP User Name")]
        Smtp_UserName,

        [Display(Name = "SMTP Password")]
        Smtp_Password,
        #endregion

    }
}
