// using System.Threading.Tasks;
// using Infrastructure.Message.Rabbit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Options;

// namespace Service.RabbitMq.PubSubClient
// {
//     public interface ITestService
//     {
//         Task PublishAsync(string message);
//     }

//     public class TestService : RabbitService, ITestService
//     {
//         public TestService(IConfiguration configuration, IOptions<RabbitOption> rabbitOptions) : base(rabbitOptions)
//         {
//             InitChannel(configuration, "Test");
//             InitOutput(exchangeName: "preprocess");
//         }

//         public Task PublishAsync(string message)
//         {
//             Publish(message, "preprocess", "");
//             return Task.CompletedTask;
//         }
//     }
// }