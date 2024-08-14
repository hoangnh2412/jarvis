using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

public class BaseEventProducer : IDistributedEventProducer
{
    protected readonly IModel Channel;
    protected readonly IConnection Connection;
    private static readonly ActivitySource ActivitySource = new ActivitySource(Assembly.GetEntryAssembly().GetName().Name);
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private readonly ILogger<BaseEventProducer> _logger;

    public BaseEventProducer(
        IRabbitMQConnector connector,
        ILogger<BaseEventProducer> logger)
    {
        Connection = connector.Connection;
        Channel = connector.Connection.CreateModel();
        _logger = logger;
    }

    public Task PublishAsync<T>(T message, string topic = null, string subject = null, IDictionary<string, string> attributes = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
        using var activity = ActivitySource.StartActivity("SendMessage", ActivityKind.Producer);
        var props = Channel.CreateBasicProperties();
        props.Persistent = true;

        // Depending on Sampling (and whether a listener is registered or not), the
        // activity above may not be created.
        // If it is created, then propagate its context.
        // If it is not created, the propagate the Current context,
        // if any.
        ActivityContext contextToInject = default;
        if (activity != null)
            contextToInject = activity.Context;
        else if (Activity.Current != null)
            contextToInject = Activity.Current.Context;

        // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), props, InjectTraceContextIntoBasicProperties);

        // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
        // See:
        //   * https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#messaging-attributes
        //   * https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/rabbitmq.md
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.rabbitmq.queue", topic);
        activity?.SetTag("messaging.rabbitmq.exhcnage", subject);


        byte[] data;
        if (message.GetType() == typeof(string))
            data = Encoding.UTF8.GetBytes(message.ToString());
        else
            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        Publish(topic, subject, props, data);
        return Task.CompletedTask;
    }

    public virtual void Publish(string topic, string subject, IBasicProperties props, byte[] data)
    {
        Channel.BasicPublish(exchange: string.IsNullOrEmpty(subject) ? String.Empty : subject, routingKey: topic, basicProperties: props, body: data);
    }

    private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        try
        {
            if (props.Headers == null)
                props.Headers = new Dictionary<string, object>();

            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}