using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Roles;

namespace Jarvis.Core.Events.Roles
{
    public interface IRoleCreatedEvent : IEvent<RoleCreatedEventModel>
    {
        
    }
}