using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class RoleClaimAction : CrudClaimAction, IClaimAction
    {
        public override string Name => "Quản lý quyền";
    }
}