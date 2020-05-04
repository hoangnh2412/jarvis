using System.Text;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Service.RabbitMq.PubSubClient
{
    public interface ITestService
    {
        Task PublishAsync(string message);
    }

    public class TestService : RabbitService, ITestService
    {
        public TestService(RabbitQueueOption queueOption, IOptions<RabbitOption> rabbitOptions) : base(queueOption, rabbitOptions)
        {
        }

        public Task PublishAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            Publish(body);
            return Task.CompletedTask;
        }
    }
}