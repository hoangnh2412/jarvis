using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application.Events;

public class EventBus : IEventBus
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventBus(
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task PublishAsync<TEvent, T>(T data) where TEvent : IEvent<T>
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var events = scope.ServiceProvider.GetServices<TEvent>();
            foreach (var item in events)
            {
                var task = item.HandleAsync(data);

                if (item.WaitUntilDone)
                    await task;
            }
        }
    }
}