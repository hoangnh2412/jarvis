using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public interface IRabbitService
    {
        void Publish<T>(T message, string exchangeName, string routingKey);
    }

    public abstract class RabbitService
    {
        protected QueueDeclareOk Queue { get; private set; }

        private readonly IRabbitBusClient _busClient;

        public RabbitService(
            IRabbitBusClient busClient)
        {
            _busClient = busClient;
        }

        protected virtual void InitExchange(string exchangeName)
        {
            _busClient.GetChannel().ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        public virtual void Publish<T>(T message, string exchangeName, string routingKey)
        {
            byte[] body;
            if (message.GetType() == typeof(string))
                body = Encoding.UTF8.GetBytes(message.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


            var properties = _busClient.GetChannel().CreateBasicProperties();
            properties.Persistent = true;

            _busClient.GetChannel().BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }
    }
}