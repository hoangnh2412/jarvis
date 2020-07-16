using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Identity;

namespace Jarvis.Core.Events.Identity
{
    public interface IIdentityPasswordResetedEvent : IEvent<IdentityPasswordResetedEventModel>
    {
        
    }
}