namespace Jarvis.Shared.Options;

public class DistributedEventOption
{
    public bool Enable { get; set; }
    public string DefaultEventBusName { get; set; }
    public EventBridgeOption EventBridge { get; set; }
}
