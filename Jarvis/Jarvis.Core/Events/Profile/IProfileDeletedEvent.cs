using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Profile;

namespace Jarvis.Core.Events.Profile
{
    public interface IProfileDeletedEvent : IEvent<ProfileDeletedEventModel>
    {
        
    }
}