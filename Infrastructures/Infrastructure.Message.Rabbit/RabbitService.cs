using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public abstract class RabbitService
    {
        protected readonly RabbitOption _rabbitOptions;
        protected RabbitQueueOption QueueOptions { get; private set; }
        protected IModel Channel { get; private set; }
        protected QueueDeclareOk Queue { get; private set; }

        public RabbitService(
            IOptions<RabbitOption> rabbitOptions)
        {
            _rabbitOptions = rabbitOptions.Value;
        }


        protected void InitChannel(IConfiguration configuration, string name)
        {
            QueueOptions = configuration.GetSection($"RabbitMq:Workers:{name}").Get<RabbitQueueOption>();
            Console.WriteLine($"Config: {JsonConvert.SerializeObject(QueueOptions)}");

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitOptions.HostName,
                UserName = _rabbitOptions.UserName,
                Password = _rabbitOptions.Password,
                Port = _rabbitOptions.Port,
                VirtualHost = _rabbitOptions.VirtualHost,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection($"{QueueOptions.ConnectionName}_{Thread.CurrentThread.ManagedThreadId}");

            Channel = connection.CreateModel();
        }

        protected virtual void InitOutput(string exchangeName)
        {
            Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
        }

        public virtual void Publish<T>(T output, string exchangeName, string routingKey)
        {
            byte[] body;
            if (output.GetType() == typeof(string))
                body = Encoding.UTF8.GetBytes(output.ToString());
            else
                body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));


            var properties = Channel.CreateBasicProperties();
            properties.Persistent = true;

            Channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
        }
    }
}