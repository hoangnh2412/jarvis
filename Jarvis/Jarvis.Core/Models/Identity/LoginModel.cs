using System.ComponentModel.DataAnnotations;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class LoginModel
    {
        [MaxLength(256, ErrorMessage = "Tên đăng nhập phải nhỏ hơn 256 ký tự")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; }

        [MaxLength(250, ErrorMessage = "Mật khẩu phải nhỏ hơn 250 ký tự")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; }
    }
}
