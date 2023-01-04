using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Users;

namespace Jarvis.Core.Events.Users
{
    public interface IUserPasswordResetedEvent : IEvent<UserPasswordResetedEventModel>
    {

    }
}