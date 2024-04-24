namespace Jarvis.Application.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T data);
}