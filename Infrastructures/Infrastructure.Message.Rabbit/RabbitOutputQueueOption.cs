namespace Infrastructure.Message.Rabbit
{
    public class RabbitOutputQueueOption
    {
        public string RoutingKey { get; set; }
        public string ExchangeName { get; set; }
    }
}