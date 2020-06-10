using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitBackgroundService<TRequest, TResponse> : BackgroundService
        where TRequest : class
        where TResponse : class
    {
        private readonly IRabbitBusClient _rabbitBusClient;

        protected IModel Channel { get; }
        protected QueueDeclareOk Queue { get; private set; }

        public RabbitBackgroundService(
            IRabbitBusClient rabbitBusClient)
        {
            _rabbitBusClient = rabbitBusClient;

            Channel = _rabbitBusClient.GetChannel();

            BasicQos();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumers = GetNumberOfconsumer();
            for (int i = 0; i < consumers; i++)
            {
                var consumer = new AsyncEventingBasicConsumer(Channel);
                consumer.Received += async (model, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body);

                    TRequest request;
                    try
                    {
                        if (typeof(TRequest) == typeof(string))
                            request = message as TRequest;
                        else
                            request = JsonConvert.DeserializeObject<TRequest>(message);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    await HandleAsync(ea, request);
                };
                Channel.BasicConsume(consumer: consumer, queue: Queue.QueueName, autoAck: false);
            }
            return Task.CompletedTask;
        }

        public virtual void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
        {
            Channel.BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
                global: global);
        }

        public virtual void InitQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            Queue = Channel.QueueDeclare(
                queue: queueName,
                durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
                arguments: arguments //TTL, queue length limit
            );
        }

        public virtual void InitExchange(string exchangeName)
        {
            Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        public virtual void InitBinding(string exchangeName, List<string> routingKeys)
        {
            foreach (var routingKey in routingKeys)
            {
                Channel.QueueBind(
                    queue: Queue.QueueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }
        }

        public virtual void Publish(PublicationAddress publicationAddress, IBasicProperties properties, TResponse message)
        {
            byte[] body;
            if (message.GetType() == typeof(string))
                body = Encoding.UTF8.GetBytes(message.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            Channel.BasicPublish(addr: publicationAddress, basicProperties: properties, body: body);
        }

        public virtual void BasicAck(BasicDeliverEventArgs ea, bool multiple = false)
        {
            Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: multiple);
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _rabbitBusClient.GetConnection().Close();
            return base.StopAsync(cancellationToken);
        }

        public abstract int GetNumberOfconsumer();

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, TRequest request);
    }
}
