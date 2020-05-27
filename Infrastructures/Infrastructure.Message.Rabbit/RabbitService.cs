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

        private readonly IRabbitBus _rabbitChannel;

        public RabbitService(
            IRabbitBus rabbitChannel)
        {
            _rabbitChannel = rabbitChannel;
        }

        protected virtual void InitExchange(string exchangeName)
        {
            _rabbitChannel.GetChannel().ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        public virtual void Publish<T>(T message, string exchangeName, string routingKey)
        {
            byte[] body;
            if (message.GetType() == typeof(string))
                body = Encoding.UTF8.GetBytes(message.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


            var properties = _rabbitChannel.GetChannel().CreateBasicProperties();
            properties.Persistent = true;

            _rabbitChannel.GetChannel().BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }
    }
}