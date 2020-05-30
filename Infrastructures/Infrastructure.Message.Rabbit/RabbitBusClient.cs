using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public interface IRabbitBusClient
    {
        IModel GetChannel();
    }

    public class RabbitBusClient : IRabbitBusClient
    {
        private readonly RabbitOption _rabbitOption;
        private readonly IModel _channel;

        public RabbitBusClient(IOptions<RabbitOption> rabbitOption)
        {
            _rabbitOption = rabbitOption.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitOption.HostName,
                UserName = _rabbitOption.UserName,
                Password = _rabbitOption.Password,
                Port = _rabbitOption.Port,
                VirtualHost = _rabbitOption.VirtualHost,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
        }

        public IModel GetChannel()
        {
            return _channel;
        }
    }
}
