using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace Infrastructure.Message.Rabbit
{
    public interface IRabbitBus
    {
        void InitChannel(string name);

        IModel GetChannel();

        IConnection GetConnection();

        RabbitQueueOption GetRabbitQueueOption();
    }

    public class RabbitBus : IRabbitBus
    {
        protected readonly RabbitOption _rabbitOptions;
        protected RabbitQueueOption QueueOptions { get; private set; }
        protected IModel Channel { get; private set; }
        protected IConnection Connection { get; private set; }

        private readonly IConfiguration _configuration;

        public RabbitBus(
            IConfiguration configuration,
            IOptions<RabbitOption> rabbitOptions)
        {
            _configuration = configuration;
            _rabbitOptions = rabbitOptions.Value;
        }

        public void InitChannel(string name)
        {
            if (Channel != null)
                return;

            QueueOptions = _configuration.GetSection($"RabbitMq:Workers:{name}").Get<RabbitQueueOption>();
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

            Connection = factory.CreateConnection($"{QueueOptions.ConnectionName}_{Thread.CurrentThread.ManagedThreadId}");

            Channel = Connection.CreateModel();
        }

        public IModel GetChannel()
        {
            return Channel;
        }

        public IConnection GetConnection()
        {
            return Connection;
        }

        public RabbitQueueOption GetRabbitQueueOption()
        {
            return QueueOptions;
        }
    }
}
