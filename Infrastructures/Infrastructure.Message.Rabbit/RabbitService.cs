using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitService<TOutput>
        where TOutput : class
    {
        protected readonly RabbitOption _rabbitOptions;
        protected RabbitQueueOption _queueOptions { get; private set; }
        protected IModel _channel { get; private set; }
        protected QueueDeclareOk _queue { get; private set; }

        public RabbitService(
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

            var connection = factory.CreateConnection($"{_queueOptions.ConnectionName}_{Thread.CurrentThread.ManagedThreadId}");

            _channel = connection.CreateModel();
        }

        protected virtual void InitOutput(string exchangeName)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
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
    }
}