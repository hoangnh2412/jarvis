using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
    public abstract class RabbitRpcClient<TRequest, TResponse> : BackgroundService
        where TRequest : class
        where TResponse : class
    {
        protected readonly RabbitOption _rabbitOptions;
        protected readonly IRabbitBus _rabbitChannel;
        protected QueueDeclareOk Queue { get; private set; }

        public RabbitRpcClient(
            IRabbitBus rabbitChannel,
            IOptions<RabbitOption> rabbitOptions)
        {
            _rabbitChannel = rabbitChannel;
            _rabbitOptions = rabbitOptions.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _rabbitChannel.GetRabbitQueueOption().NumberOfConsumer; i++)
            {
                var consumer = new AsyncEventingBasicConsumer(_rabbitChannel.GetChannel());
                consumer.Received += async (model, ea) =>
                {
                    RabbitResponseModel<TResponse> response = null;
                    var props = ea.BasicProperties;
                    var replyProps = _rabbitChannel.GetChannel().CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(ea.Body);
                        var request = JsonConvert.DeserializeObject<TRequest>(message);

                        response = new RabbitResponseModel<TResponse>
                        {
                            Data = await HandleRequestAsync(request),
                            Succeeded = true,
                            Exception = null
                        };
                    }
                    catch (Exception ex)
                    {
                        response = new RabbitResponseModel<TResponse>
                        {
                            Exception = ex,
                            Succeeded = false,
                            Data = null
                        };
                    }
                    finally
                    {
                        _rabbitChannel.GetChannel().BasicPublish(props.ReplyToAddress, replyProps, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                        _rabbitChannel.GetChannel().BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };
                _rabbitChannel.GetChannel().BasicConsume(queue: Queue.QueueName, autoAck: false, consumer: consumer);
            }
            return Task.CompletedTask;
        }

        protected void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
        {
            _rabbitChannel.GetChannel().BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
                global: global);
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

        protected virtual void InitInput(string exchangeName, List<string> routingKeys)
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

        public abstract Task<TResponse> HandleRequestAsync(TRequest request);
    }
}
