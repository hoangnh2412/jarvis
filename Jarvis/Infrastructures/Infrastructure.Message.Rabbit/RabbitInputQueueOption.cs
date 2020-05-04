using System.Collections.Generic;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitInputQueueOption
    {
        public List<string> RoutingKey { get; set; }
        public string ExchangeName { get; set; }
    }
}