using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Tenant
{
    public class TenantUserModel
    {
        [Required(ErrorMessage = "Tên đầy đủ tài khoản không được để trống")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không dài quá 100 ký tự")]
        public string Password { get; set; }

        //[Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [MaxLength(100, ErrorMessage = "Xác nhận mật khẩu không dài quá 100 ký tự")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// email bắt buộc có khi chọn mật khẩu tự động
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [MaxLength(500, ErrorMessage = "Email không dài quá 500 ký tự")]
        public string Email { get; set; }
    }
}
