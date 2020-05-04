using System.Collections.Generic;
using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class SettingClaimAction : IClaimAction
    {
        public List<ClaimAction> Actions = new List<ClaimAction>();
        public string Name => "Quản lý tham số";

        public SettingClaimAction()
        {
            Actions = new List<ClaimAction> {
                new ClaimAction { Id = 1, Name = "Read", Description = "Xem" },
                new ClaimAction { Id = 1, Name = "Update", Description = "Sửa" }
            };
        }

        public List<ClaimAction> GetActions()
        {
            return Actions;
        }
    }
}