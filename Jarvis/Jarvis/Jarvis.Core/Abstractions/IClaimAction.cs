using System.Collections.Generic;

namespace Jarvis.Core.Abstractions
{
    public interface IClaimAction
    {
        string Name { get; }

        List<ClaimAction> GetActions();
    }
}