using System.Collections.Generic;
using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class OrganizationClaimAction : CrudClaimAction, IClaimAction
    {
        public override string Name => "Quản lý phòng ban";

        public OrganizationClaimAction()
        {
            Actions = new List<ClaimAction> {
                new ClaimAction { Id = 16, Name = "Users", Description = "Thành viên" },
                new ClaimAction { Id = 32, Name = "Roles", Description = "Quyền" }
            };
        }
    }
}