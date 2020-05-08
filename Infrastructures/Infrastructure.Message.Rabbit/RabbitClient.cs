using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitClient<TInput, TOutput> : BackgroundService
        where TInput : class
        where TOutput : class

    {
        protected readonly RabbitQueueOption _queueOptions;
        protected readonly RabbitOption _rabbitOptions;
        protected readonly IModel _channel;

        public RabbitClient(
            string name,
            IConfiguration configuration,
            IOptions<RabbitOption> rabbitOptions)
        {
            _queueOptions = configuration.GetSection(name).Get<RabbitQueueOption>();
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
                    var message = Encoding.UTF8.GetString(ea.Body);

                    TInput input;
                    if (typeof(TInput) == typeof(String))
                        input = message as TInput;
                    else
                        input = JsonConvert.DeserializeObject<TInput>(message);

                    await HandleAsync(ea, input);
                };
                _channel.BasicConsume(
                    queue: _queueOptions.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            return Task.CompletedTask;
        }

        public void BasicAck(BasicDeliverEventArgs ea)
        {
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }

        public virtual void Publish(TOutput message)
        {
            Publish(message, () =>
            {
                return _queueOptions.Output.ExchangeName;
            }, () =>
            {
                return _queueOptions.Output.RoutingKey;
            });
        }

        public virtual void Publish(TOutput output, Func<string> exchangeName, Func<string> routingKey)
        {
            if (_queueOptions.Output == null)
                return;

            byte[] body;
            if (output.GetType() == typeof(String))
                body = Encoding.UTF8.GetBytes(output.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));


            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName.Invoke(), routingKey: routingKey.Invoke(), basicProperties: properties, body: body);
        }

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, TInput input);

    }
}
