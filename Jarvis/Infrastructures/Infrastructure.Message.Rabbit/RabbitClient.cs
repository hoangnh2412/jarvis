using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitClient : BackgroundService
    {
        private readonly RabbitQueueOption _queueOptions;
        private readonly RabbitOption _rabbitOptions;
        private readonly IModel _channel;

        public RabbitClient(
            RabbitQueueOption queueOption,
            IOptions<RabbitOption> rabbitOptions)
        {
            _queueOptions = queueOption;
            _rabbitOptions = rabbitOptions.Value;
            Console.WriteLine($"QueueName: {_queueOptions.QueueName}");

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

            //Dequeue mỗi lần 1 message
            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false);

            //Input
            _channel.QueueDeclare(
                queue: _queueOptions.QueueName,
                durable: true, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: false, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: false, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
                arguments: null); //TTL, queue length limit

            _channel.ExchangeDeclare(exchange: _queueOptions.Input.ExchangeName, type: ExchangeType.Topic);
            foreach (var routingKey in _queueOptions.Input.RoutingKey)
            {
                _channel.QueueBind(
                    queue: _queueOptions.QueueName,
                    exchange: _queueOptions.Input.ExchangeName,
                    routingKey: routingKey);
            }

            //Output
            if (_queueOptions.Output != null)
            {
                _channel.ExchangeDeclare(exchange: _queueOptions.Output.ExchangeName, type: ExchangeType.Topic);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _queueOptions.NumberOfConsumer; i++)
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    await HandleAsync(ea, _channel);
                };
                _channel.BasicConsume(
                    queue: _queueOptions.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            return Task.CompletedTask;
        }

        public void TagDeliveryMessage(BasicDeliverEventArgs ea, IModel channel)
        {
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }

        public virtual void Publish(BasicDeliverEventArgs ea, IModel channel, Func<string> handleMessage)
        {
            if (_queueOptions.Output == null)
                return;

            var body = Encoding.UTF8.GetBytes(handleMessage());

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: _queueOptions.Output.ExchangeName, routingKey: _queueOptions.Output.RoutingKey, basicProperties: properties, body: body);
        }

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, IModel channel);
    }
}
