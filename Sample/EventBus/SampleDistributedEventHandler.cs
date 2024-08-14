using Jarvis.Infrastructure.DistributedEvent.RabbitMQ;
using Newtonsoft.Json;

namespace Sample.EventBus;

public class SampleDistributedEventHandler : BaseEventConsumer<SampleEto>
{
    private readonly ILogger<SampleDistributedEventHandler> _logger;

    public SampleDistributedEventHandler(
        IRabbitMQConnector connector,
        ILogger<SampleDistributedEventHandler> logger)
        : base(connector, logger)
    {
        _logger = logger;
    }

    public override DeclareOption DeclareOption => new DeclareOption("sample-queue", "sample", "all")
    {
        AutoAck = false
    };

    public override async Task HandleAsync(SampleEto data)
    {
        _logger.LogDebug(JsonConvert.SerializeObject(data));

        try
        {
            Console.WriteLine(data.Data);
            await Task.Yield();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            Ack();
        }
    }
}