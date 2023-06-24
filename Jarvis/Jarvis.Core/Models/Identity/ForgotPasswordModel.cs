using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Identity
{
    public class ForgotPasswordModel
    {
        /// <summary>
        /// email người nhận
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; }
    }
}
