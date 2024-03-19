namespace Jarvis.Infrastructure.DistributedEvent;

public interface IDistributedEventBus
{
    /// <summary>
    /// Publish message with type is implement from IBaseEventMessage
    /// </summary>
    /// <param name="message"></param>
    /// <param name="topic"></param>
    /// <param name="subject"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    Task PublishAsync(IBaseEventMessage message, string topic, string subject = null, IDictionary<string, string> attributes = null);
}