using System.Collections.Generic;
using System.Text;
using Infrastructure.Message.Rabbit.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Message.Rabbit
{
    public class BusService : IBusService
    {
        private readonly IEventBusConnector _connector;
        private readonly IModel _channel;
        private readonly RabbitOption _options;

        public BusService(
            IEventBusConnector connector,
            IOptions<RabbitOption> rabbitOptions)
        {
            _options = rabbitOptions.Value;
            _connector = connector;
            _channel = connector.CreateChannel();
        }

        public IModel GetChannel()
        {
            return _channel;
        }

        public void InitQos(uint prefetchSize = 0, ushort prefetchCount = 50, bool global = false)
        {
            _channel.BasicQos(
                prefetchSize: prefetchSize, //Load trước message
                prefetchCount: prefetchCount, //Dequeue mỗi lần 50 message
                global: global);
        }

        public void CreateExchange(string exchangeName)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        public void BasicAck(BasicDeliverEventArgs ea, bool multiple = false)
        {
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple);
        }

        public QueueDeclareOk InitQueue(string queueName, string exchangeName, List<string> routingKeys, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            var queue = _channel.QueueDeclare(
                queue: queueName,
                durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe hoặc queue trống)
                arguments: arguments //TTL, queue length limit
            );

            foreach (var routingKey in routingKeys)
            {
                _channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }

            return queue;
        }

        public void Publish<T>(T message, string exchangeName, string routingKey, bool persistent = true)
        {
            byte[] body;
            if (message.GetType() == typeof(string))
                body = Encoding.UTF8.GetBytes(message.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = persistent;

            _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _channel.Dispose();
        }
    }
}
