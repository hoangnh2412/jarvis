using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Message.Rabbit.Abstractions
{
    /// <summary>
    /// A channel connect to RabbitMQ for each thread. Recommend register lifetime DI is scoped
    /// </summary>
    public interface IBusService : IDisposable
    {
        /// <summary>
        /// Get current channel
        /// </summary>
        /// <returns></returns>
        IModel GetChannel();

        /// <summary>
        /// Init QoS for current channel
        /// </summary>
        /// <param name="prefetchSize"></param>
        /// <param name="prefetchCount"></param>
        /// <param name="global"></param>
        void InitQos(uint prefetchSize = 0, ushort prefetchCount = 50, bool global = false);

        /// <summary>
        /// Create Exchange
        /// </summary>
        /// <param name="exchangeName"></param>
        void CreateExchange(string exchangeName);

        /// <summary>
        /// ACK for 
        /// </summary>
        /// <param name="ea"></param>
        /// <param name="multiple"></param>
        void BasicAck(BasicDeliverEventArgs ea, bool multiple = false);

        /// <summary>
        /// Create Queue and binding to exchange
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKeys"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="Delete"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        QueueDeclareOk InitQueue(string queueName, string exchangeName, List<string> routingKeys, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null);

        void Publish<T>(T message, string exchangeName, string routingKey, bool persistent = true);
    }
}
