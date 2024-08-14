namespace Jarvis.Infrastructure.DistributedEvent;

public interface IDistributedEventBusConnector
{
    IDistributedEventConnection Connect();

    void Disconnect();
}