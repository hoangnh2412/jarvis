using System;
using System.Collections.Generic;
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
        protected readonly RabbitOption _rabbitOptions;
        protected RabbitQueueOption _queueOptions { get; private set; }
        protected IModel _channel { get; private set; }
        protected IConnection _connection { get; private set; }
        protected QueueDeclareOk _queue { get; private set; }

        public RabbitClient(
            IConfiguration configuration,
            IOptions<RabbitOption> rabbitOptions)
        {
            _rabbitOptions = rabbitOptions.Value;
        }

        protected void InitChannel(IConfiguration configuration, string name)
        {
            _queueOptions = configuration.GetSection($"RabbitMq:Workers:{name}").Get<RabbitQueueOption>();
            Console.WriteLine($"Config: {JsonConvert.SerializeObject(_queueOptions)}");

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitOptions.HostName,
                UserName = _rabbitOptions.UserName,
                Password = _rabbitOptions.Password,
                Port = _rabbitOptions.Port,
                VirtualHost = _rabbitOptions.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection($"{_queueOptions.ConnectionName}_{Thread.CurrentThread.ManagedThreadId}");

            _channel = _connection.CreateModel();
        }

        protected void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
        {
            _channel.BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
                global: global);
        }

        protected virtual void InitInput(string exchangeName, List<string> routingKeys)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
            foreach (var routingKey in routingKeys)
            {
                _channel.QueueBind(
                    queue: _queue.QueueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }
        }

        protected virtual void InitQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _queue = _channel.QueueDeclare(
                queue: queueName,
                durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
                arguments: arguments //TTL, queue length limit
            );
        }

        protected virtual void InitOutput(string exchangeName)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
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
                    queue: _queue.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            return Task.CompletedTask;
        }

        public void BasicAck(BasicDeliverEventArgs ea, bool multiple = false)
        {
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple);
        }

        public virtual void Publish(TOutput output, string exchangeName, string routingKey)
        {
            byte[] body;
            if (output.GetType() == typeof(String))
                body = Encoding.UTF8.GetBytes(output.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));


            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }

        public virtual void Publish(TOutput output, Func<string> exchangeName, Func<string> routingKey)
        {
            Publish(output, exchangeName.Invoke(), routingKey.Invoke());
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _connection.Close();
            return base.StopAsync(cancellationToken);
        }

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, TInput input);

    }
}
