namespace Jarvis.Infrastructure.DistributedEvent.EventBridge;

public class BaseEventMessage : IBaseEventMessage
{
    public string Id { get; set; }
    public string Action { get; set; }
    public string EntityName { get; set; }
    public object EntityData { get; set; }
    public string Sender { get; set; }
}