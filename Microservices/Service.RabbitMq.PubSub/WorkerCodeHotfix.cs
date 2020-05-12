using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Service.RabbitMq.PubSub
{
    public class WorkerCodeHotfix : RabbitClient<string, string>
    {
        public WorkerCodeHotfix(IConfiguration configuration, IOptions<RabbitOption> rabbitOptions) : base(configuration, rabbitOptions)
        {
            InitChannel(configuration, "CodeHotfix");
            BasicQos();
            InitQueue(queueName: "code-hotfix");
            InitInput(exchangeName: "working", routingKeys: new List<string> { "*.*.code.hotfix", "*.*.code.hotfix-release" });
            InitOutput(exchangeName: "sleep");
        }

        public override async Task HandleAsync(BasicDeliverEventArgs ea, string message)
        {
            try
            {
                Console.WriteLine($" [>] {_queueOptions.ConnectionName} Received: {message}");

                var number = int.Parse(message) + 7;
                Console.WriteLine($" [...] {_queueOptions.ConnectionName} working on {number}s");
                await Task.Delay(number * 1000);

                Publish(message, "sleep", "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                BasicAck(ea);
            }
        }
    }
}
