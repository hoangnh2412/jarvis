using System.ComponentModel.DataAnnotations;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không dài quá 100 ký tự")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [MaxLength(100, ErrorMessage = "Xác nhận mật khẩu không dài quá 100 ký tự")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
