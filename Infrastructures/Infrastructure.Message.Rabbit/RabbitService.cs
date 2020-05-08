using System.Threading;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitService
    {
        protected readonly RabbitQueueOption _queueOptions;
        protected readonly RabbitOption _rabbitOptions;
        protected readonly IModel _channel;

        public RabbitService(
            RabbitQueueOption queueOption,
            IOptions<RabbitOption> rabbitOptions)
        {
            _queueOptions = queueOption;
            _rabbitOptions = rabbitOptions.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitOptions.HostName,
                UserName = _rabbitOptions.UserName,
                Password = _rabbitOptions.Password,
                Port = _rabbitOptions.Port,
                VirtualHost = _rabbitOptions.VirtualHost,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection($"{_queueOptions.ConnectionName}_{Thread.CurrentThread.ManagedThreadId}");
            _channel = connection.CreateModel();

            //Output
            if (_queueOptions.Output != null)
            {
                _channel.ExchangeDeclare(exchange: _queueOptions.Output.ExchangeName, type: ExchangeType.Topic);
            }
        }

        public virtual void Publish(byte[] bytes)
        {
            _channel.BasicPublish(
                exchange: _queueOptions.Output.ExchangeName,
                routingKey: _queueOptions.Output.RoutingKey,
                basicProperties: null,
                body: bytes);
        }
    }
}