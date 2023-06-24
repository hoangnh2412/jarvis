// using Microsoft.Extensions.Options;
// using RabbitMQ.Client;

// namespace Infrastructure.Message.Rabbit
// {
//     public interface IRabbitBusClient
//     {
//         IModel GetChannel();

//         IConnection GetConnection();
//     }

//     public class RabbitBusClient : IRabbitBusClient
//     {
//         private readonly RabbitOption _rabbitOption;
//         private readonly IModel _channel;
//         private readonly IConnection _connection;

//         public RabbitBusClient(IOptions<RabbitOption> rabbitOption)
//         {
//             _rabbitOption = rabbitOption.Value;

//             var factory = new ConnectionFactory()
//             {
//                 HostName = _rabbitOption.HostName,
//                 UserName = _rabbitOption.UserName,
//                 Password = _rabbitOption.Password,
//                 Port = _rabbitOption.Port,
//                 VirtualHost = _rabbitOption.VirtualHost,
//                 DispatchConsumersAsync = true
//             };

//             _connection = factory.CreateConnection();
//             _channel = _connection.CreateModel();

//             _channel.ExchangeDeclare(exchange: RabbitKey.Exchanges.Events, type: ExchangeType.Topic);
//             _channel.ExchangeDeclare(exchange: RabbitKey.Exchanges.Commands, type: ExchangeType.Topic);
//             _channel.ExchangeDeclare(exchange: RabbitKey.Exchanges.Queries, type: ExchangeType.Topic);
//             _channel.ExchangeDeclare(exchange: RabbitKey.Exchanges.Rpc, type: ExchangeType.Topic);
//         }

//         public IModel GetChannel()
//         {
//             return _channel;
//         }

//         public IConnection GetConnection()
//         {
//             return _connection;
//         }
//     }
// }
