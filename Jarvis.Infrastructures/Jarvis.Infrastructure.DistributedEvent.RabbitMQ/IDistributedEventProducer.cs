namespace Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

public interface IDistributedEventProducer
{
    Task PublishAsync<T>(T message, string topic = null, string subject = null, IDictionary<string, string> attributes = null);
}