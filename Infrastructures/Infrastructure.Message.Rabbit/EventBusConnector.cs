using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Infrastructure.Message.Rabbit.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit
{
    public class EventBusConnector : IEventBusConnector
    {
        private IConnection _connection;
        private readonly RabbitOption _options;

        public EventBusConnector(
            IOptions<RabbitOption> options)
        {
            _options = options.Value;
            var factory = new ConnectionFactory()
            {
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            var hosts = _options.Hosts.Select(x => new AmqpTcpEndpoint(x.HostName, x.Port)).ToList();
            _connection = factory.CreateConnection(hosts);
        }

        public IModel CreateChannel()
        {
            return _connection.CreateModel();
        }

        public void Disconnect()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}