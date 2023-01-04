using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Tenants;

namespace Jarvis.Core.Events.Tenants
{
    public interface ITenantInfoUpdatedEvent : IEvent<TenantInfoUpdatedEventModel>
    {
        
    }
}