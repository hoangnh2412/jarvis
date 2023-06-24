using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit.Abstractions;
using Microsoft.Extensions.Configuration;
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
        protected bool _autoAck { get; set; }
        protected int _threads { get; set; }
        protected string _queueName { get; set; }

        protected IModel _channel { get; set; }
        private readonly IBusService _busService;

        public RabbitClient(
            IConfiguration configuration,
            IBusService busService)
        {
            _busService = busService;
            _channel = _busService.GetChannel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_threads == 0)
                _threads = 1;

            for (int i = 0; i < _threads; i++)
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
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
                _channel.BasicConsume(
                    queue: _queueName,
                    autoAck: _autoAck,
                    consumer: consumer);
            }
            return Task.CompletedTask;
        }

        public void BasicAck(BasicDeliverEventArgs ea, bool multiple = false)
        {
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple);
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

        public virtual void Publish(TOutput output, Func<string> exchangeName, Func<string> routingKey)
        {
            Publish(output, exchangeName.Invoke(), routingKey.Invoke());
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _busService.Dispose();
            return base.StopAsync(cancellationToken);
        }

        public abstract Task HandleAsync(BasicDeliverEventArgs ea, TInput input);

    }
}
