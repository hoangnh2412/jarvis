using RabbitMQ.Client;

namespace Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

public class DeclareOption
{
    public DeclareOption(string queueName)
    {
        Queue = new QueueOption(queueName);
    }

    public DeclareOption(string queueName, string exchangeName, string routingKey)
    {
        Queue = new QueueOption(queueName);
        Binding = new List<BindingOption> {
            new BindingOption(exchangeName, routingKey)
        };
    }

    public DeclareOption(string queueName, string exchangeName, params string[] routingKeys)
    {
        Queue = new QueueOption(queueName);
        Binding = routingKeys.Select(x => new BindingOption(exchangeName, x)).ToList();
    }

    public QueueOption Queue { get; set; }
    public IList<BindingOption> Binding { get; set; }
    public QosOption QoS { get; set; } = new QosOption();
    public bool AutoAck { get; set; } = true;
    public int Threads { get; set; } = 1;

    public class ExchangeOption
    {
        public ExchangeOption(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string Type { get; set; } = ExchangeType.Topic;
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }
    }

    public class QueueOption
    {
        public QueueOption(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }
    }

    public class QosOption
    {
        public uint PrefetchSize { get; set; } = 0;
        public ushort PrefetchCount { get; set; } = 1;
        public bool Global { get; set; } = false;
    }

    public class BindingOption
    {
        public BindingOption(string exchangeName, string routingKey)
        {
            Exchange = new ExchangeOption(exchangeName);
            RoutingKey = routingKey;
        }

        public ExchangeOption Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
    }
}