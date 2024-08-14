namespace Jarvis.Application.Events;

public interface IEvent<T>
{
    bool WaitUntilDone { get; }

    Task HandleAsync(T data);
}