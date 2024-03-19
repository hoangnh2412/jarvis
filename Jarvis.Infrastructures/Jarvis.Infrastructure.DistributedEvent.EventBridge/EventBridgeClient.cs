using Amazon;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Jarvis.Shared.Options;

namespace Jarvis.Infrastructure.DistributedEvent.EventBridge;

public class EventBridgeClient : IDistributedEventBus
{
    public readonly AmazonEventBridgeClient Client;
    private readonly EventBridgeOption _options;

    public EventBridgeClient(
        IOptions<EventBridgeOption> options)
    {
        _options = options.Value;

        Client = new AmazonEventBridgeClient(
            awsAccessKeyId: _options.AccessKey,
            awsSecretAccessKey: _options.SecretKey,
            region: RegionEndpoint.GetBySystemName(_options.Region)
        );
    }

    public async Task PublishAsync(IBaseEventMessage message, string topic, string subject = null, IDictionary<string, string> attributes = null)
    {
        await Client.PutEventsAsync(new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
            {
                new PutEventsRequestEntry {
                    Detail = JsonConvert.SerializeObject(message),
                    DetailType = "text/json",
                    EventBusName = topic,
                    Time = DateTime.UtcNow,
                    Source = string.IsNullOrEmpty(subject) ? message.Sender : subject
                }
            }
        });
    }
}