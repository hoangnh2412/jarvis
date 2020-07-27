using Einvoice.Utils.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Identity
{
    public class UpdateUserModel
    {
        public string PhoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "Email tài khoản dài tối đa 250 ký tự")]
        [EmailAttribute(ErrorMessage = "Email tài khoản không đúng định dạng email")]
        public string Email { get; set; }

        public string Metadata { get; set; }

        public UpdateUserInfoModel Infos { get; set; }

        public List<Guid> IdRoles { get; set; }
    }

    public class UpdateUserInfoModel
    {
        [Required(ErrorMessage = "Họ tên tài khoản không được để trống")]
        public string FullName { get; set; }

        public string AvatarPath { get; set; }
    }
}
