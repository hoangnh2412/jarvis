using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class TenantClaimAction : CrudClaimAction, IClaimAction
    {
        public override string Name => "Quản lý đơn vị";
    }
}