using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class LabelClaimAction : CrudClaimAction, IClaimAction
    {
        public override string Name => "Quản lý nhãn";
    }
}