// using Newtonsoft.Json;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;

// namespace Infrastructure.Message.Rabbit
// {
//     public interface IRabbitRpcService<TRequest, TResponse>
//         where TRequest : class
//         where TResponse : class
//     {
//         Task<TResponse> PublishAsync(TRequest message, PublicationAddress requestPublicationAddress, PublicationAddress responsePublicationAddress, CancellationToken cancellationToken = default);
//     }

//     public abstract class RabbitRpcService<TRequest, TResponse>
//         where TRequest : class
//         where TResponse : class
//     {
//         protected readonly RabbitOption _rabbitOptions;
//         protected readonly IRabbitBusClient _busClient;
//         protected IModel Channel { get; private set; }
//         protected QueueDeclareOk Queue { get; private set; }
//         protected ConcurrentDictionary<string, TaskCompletionSource<TResponse>> ResponseQueue = new ConcurrentDictionary<string, TaskCompletionSource<TResponse>>();
//         protected AsyncEventingBasicConsumer Consumer { get; private set; }

//         public RabbitRpcService(
//             IRabbitBusClient busClient)
//         {
//             _busClient = busClient;
//             Channel = _busClient.GetChannel();
//         }

//         protected virtual void InitQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
//         {
//             Queue = Channel.QueueDeclare(
//                 queue: queueName,
//                 durable: durable, //The queue will survive when RabbitMQ restart (Queue vẫn tồn tại/sống sót sau khi RabbitMQ/broker bị restart)
//                 exclusive: exclusive, //The queue will be deleted when that connection closes (Queue sẽ bị xoá khi connection close)
//                 autoDelete: autoDelete, //Queue that has had at least one consumer is deleted when last consumer unsubscribes (Queue sẽ bị xoá khi consumer unsubcribe)
//                 arguments: arguments //TTL, queue length limit
//             );
//         }

//         protected void BasicQos(uint prefetchSize = 0, ushort prefetchCount = 1, bool global = false)
//         {
//             Channel.BasicQos(
//                 prefetchSize: prefetchSize,
//                 prefetchCount: prefetchCount, //Dequeue mỗi lần 1 message
//                 global: global);
//         }

//         protected void InitConsumer()
//         {
//             Consumer = new AsyncEventingBasicConsumer(Channel);
//             Consumer.Received += async (model, ea) =>
//             {
//                 if (!ResponseQueue.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<TResponse> tcs))
//                     return;

//                 try
//                 {
//                     var body = ea.Body;
//                     var response = Encoding.UTF8.GetString(body.ToArray());
//                     tcs.TrySetResult(JsonConvert.DeserializeObject<TResponse>(response));
//                     await Task.Yield();
//                 }
//                 catch (Exception ex)
//                 {
//                     throw ex;
//                 }
//                 finally
//                 {
//                     Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                 }
//             };
//             Channel.BasicConsume(queue: Queue.QueueName, autoAck: false, consumer: Consumer);
//         }

//         protected virtual void InitInput(string exchangeName, List<string> routingKeys)
//         {
//             Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
//             foreach (var routingKey in routingKeys)
//             {
//                 Channel.QueueBind(
//                     queue: Queue.QueueName,
//                     exchange: exchangeName,
//                     routingKey: routingKey);
//             }
//         }

//         public Task<TResponse> PublishAsync(TRequest message, PublicationAddress requestPublicationAddress, PublicationAddress responsePublicationAddress, CancellationToken cancellationToken = default)
//         {
//             var props = Channel.CreateBasicProperties();
//             var correlationId = Guid.NewGuid().ToString();
//             props.CorrelationId = correlationId;
//             props.ReplyToAddress = responsePublicationAddress;

//             var tcs = new TaskCompletionSource<TResponse>();
//             ResponseQueue.TryAdd(props.CorrelationId, tcs);

//             Channel.BasicPublish(
//                 addr: requestPublicationAddress,
//                 basicProperties: props,
//                 body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))
//             );

//             cancellationToken.Register(() => ResponseQueue.TryRemove(props.CorrelationId, out var tmp));
//             return tcs.Task;
//         }
//     }
// }
