using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
        private readonly IRabbitChannel _rabbitChannel;
        protected QueueDeclareOk Queue { get; private set; }

        public RabbitClient(
            IRabbitChannel rabbitChannel)
        {
            _rabbitChannel = rabbitChannel;
        }

        protected void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
        {
            _rabbitChannel.GetChannel().BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
                global: global);
        }

        protected virtual void InitBinding(string exchangeName, List<string> routingKeys)
        {
            _rabbitChannel.GetChannel().ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
            foreach (var routingKey in routingKeys)
            {
                _rabbitChannel.GetChannel().QueueBind(
                    queue: Queue.QueueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }
        }

        protected virtual void InitQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            Queue = _rabbitChannel.GetChannel().QueueDeclare(
                queue: queueName,
                durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
                arguments: arguments //TTL, queue length limit
            );
        }

        protected virtual void InitExchange(string exchangeName)
        {
            _rabbitChannel.GetChannel().ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _rabbitChannel.GetRabbitQueueOption().NumberOfConsumer; i++)
            {
                var consumer = new AsyncEventingBasicConsumer(_rabbitChannel.GetChannel());
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
                _rabbitChannel.GetChannel().BasicConsume(
                    queue: Queue.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            return Task.CompletedTask;
        }

        public void BasicAck(BasicDeliverEventArgs ea, bool multiple = false)
        {
            _rabbitChannel.GetChannel().BasicAck(deliveryTag: ea.DeliveryTag, multiple);
        }

        public virtual void Publish(TOutput output, string exchangeName, string routingKey)
        {
            byte[] body;
            if (output.GetType() == typeof(String))
                body = Encoding.UTF8.GetBytes(output.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));


            var properties = _rabbitChannel.GetChannel().CreateBasicProperties();
            properties.Persistent = true;

            _rabbitChannel.GetChannel().BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }

        public virtual void Publish(TOutput output, Func<string> exchangeName, Func<string> routingKey)
        {
            Publish(output, exchangeName.Invoke(), routingKey.Invoke());
        }

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, TInput input);
    }
}
