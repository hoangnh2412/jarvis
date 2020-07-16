using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Labels;

namespace Jarvis.Core.Events.Labels
{
    public interface ILabelDeletedEvent : IEvent<LabelDeletedEventModel>
    {
        
    }
}