namespace Jarvis.Application.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent, T>(T data) where TEvent : IEvent<T>;
}