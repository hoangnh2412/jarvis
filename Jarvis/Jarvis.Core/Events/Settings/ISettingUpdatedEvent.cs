using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Settings;

namespace Jarvis.Core.Events.Settings
{
    public interface ISettingUpdatedEvent : IEvent<SettingUpdatedEventModel>
    {
        
    }
}