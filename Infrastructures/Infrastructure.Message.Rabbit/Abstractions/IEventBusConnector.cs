using RabbitMQ.Client;

namespace Infrastructure.Message.Rabbit.Abstractions
{
    public interface IEventBusConnector
    {
        void Disconnect();

        IModel CreateChannel();
    }
}