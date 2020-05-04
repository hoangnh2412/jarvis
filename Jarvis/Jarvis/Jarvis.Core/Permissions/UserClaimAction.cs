using System.Collections.Generic;
using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class UserClaimAction : CrudClaimAction, IClaimAction
    {
        public override string Name => "Quản lý người dùng";

        public UserClaimAction()
        {
            Actions = new List<ClaimAction> {
                new ClaimAction { Id = 16, Name = "Lock/Unlock", Description = "Khoá/Mở khoá" },
                new ClaimAction { Id = 32, Name = "ResetPassword", Description = "Thiết lập lại mật khẩu" }
            };
        }
    }
}