using System;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Identity
{
    public class ResetForgotPasswordModel
    {
        public Guid Id { get; set; }


        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không dài quá 100 ký tự")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Mã bảo mật không được để trống")]
        public string SecurityStamp { get; set; }
    }
}
