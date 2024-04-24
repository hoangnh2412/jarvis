using Jarvis.Application.Events;

namespace Sample.EventBus;

public class SsmpleEventHandler : IEvent<SampleEto>
{
    public bool WaitUntilDone => true;

    public Task HandleAsync(SampleEto data)
    {
        Console.WriteLine(data.Data);
        return Task.CompletedTask;
    }
}