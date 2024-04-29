using Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

namespace Sample.EventBus;

public class SampleDistributedEventHandler : BaseEventConsumer<SampleEto>
{
    public SampleDistributedEventHandler(
        IRabbitMQConnector connector,
        ILogger<SampleDistributedEventHandler> logger)
        : base(connector, logger)
    {
    }

    public override DeclareOption DeclareOption => new DeclareOption("sample-queue", "sample", "all");

    public override Task HandleAsync(SampleEto data)
    {
        Console.WriteLine(data.Data);
        return Task.CompletedTask;
    }
}