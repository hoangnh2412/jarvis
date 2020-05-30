using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Message.Rabbit
{
    public interface IRabbitRpcService<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        Task<RabbitRpcResponseModel<TResponse>> PublishAsync(TRequest message, string exchangeName, string routingKey, CancellationToken cancellationToken = default);
    }

    public abstract class RabbitRpcService<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        protected readonly RabbitOption _rabbitOptions;
        protected readonly IRabbitBusClient _busClient;
        protected QueueDeclareOk Queue { get; private set; }
        protected IBasicProperties Props { get; private set; }
        protected ConcurrentDictionary<string, TaskCompletionSource<RabbitRpcResponseModel<TResponse>>> ResponseQueue = new ConcurrentDictionary<string, TaskCompletionSource<RabbitRpcResponseModel<TResponse>>>();
        protected AsyncEventingBasicConsumer Consumer { get; private set; }

        public RabbitRpcService(
            IRabbitBusClient busClient)
        {
            _busClient = busClient;
        }

        protected virtual void InitQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            Queue = _busClient.GetChannel().QueueDeclare(
                queue: queueName,
                durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
                exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
                autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
                arguments: arguments //TTL, queue length limit
            );
        }

        protected void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
        {
            _busClient.GetChannel().BasicQos(
                prefetchSize: prefetchSize,
                prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
                global: global);
        }

        protected void InitConsumer(string exchangeName, string routingKey)
        {
            Props = _busClient.GetChannel().CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            Props.CorrelationId = correlationId;
            //Props.ReplyTo = Queue.QueueName;
            Props.ReplyToAddress = new PublicationAddress(
                exchangeType: ExchangeType.Topic, 
                exchangeName: exchangeName,
                routingKey: routingKey);

            Consumer = new AsyncEventingBasicConsumer(_busClient.GetChannel());
            Consumer.Received += async (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId != correlationId)
                    return;

                if (!ResponseQueue.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<RabbitRpcResponseModel<TResponse>> tcs))
                    return;

                try
                {
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body.ToArray());
                    tcs.TrySetResult(JsonConvert.DeserializeObject<RabbitRpcResponseModel<TResponse>>(response));
                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _busClient.GetChannel().BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _busClient.GetChannel().BasicConsume(queue: Queue.QueueName, autoAck: false, consumer: Consumer);
        }

        protected virtual void InitInput(string exchangeName, List<string> routingKeys)
        {
            _busClient.GetChannel().ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
            foreach (var routingKey in routingKeys)
            {
                _busClient.GetChannel().QueueBind(
                    queue: Queue.QueueName,
                    exchange: exchangeName,
                    routingKey: routingKey);
            }
        }

        public Task<RabbitRpcResponseModel<TResponse>> PublishAsync(TRequest message, string exchangeName, string routingKey, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<RabbitRpcResponseModel<TResponse>>();
            ResponseQueue.TryAdd(Props.CorrelationId, tcs);

            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _busClient.GetChannel().BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: Props, body: messageBytes);
            //Channel.BasicConsume(queue: Queue.QueueName, autoAck: false, consumer: Consumer);

            cancellationToken.Register(() => ResponseQueue.TryRemove(Props.CorrelationId, out var tmp));
            return tcs.Task;
        }
    }
}
