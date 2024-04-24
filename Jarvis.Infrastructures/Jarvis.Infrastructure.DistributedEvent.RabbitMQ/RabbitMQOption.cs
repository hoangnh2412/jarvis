namespace Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

public class RabbitMQOption
{
    public List<RabbitHostName> Hosts { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }

    public class RabbitHostName
    {
        public string HostName { get; set; }
        public int Port { get; set; }
    }
}