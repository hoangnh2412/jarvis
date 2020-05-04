using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Service.RabbitMq.PubSub
{
    public class WorkerS2TPriorityAnalyze : RabbitClient
    {
        private readonly RabbitQueueOption _queueOption;

        public WorkerS2TPriorityAnalyze(RabbitQueueOption queueOption, IOptions<RabbitOption> rabbitOptions) : base(queueOption, rabbitOptions)
        {
            _queueOption = queueOption;
        }

        public override async Task HandleAsync(BasicDeliverEventArgs ea, IModel channel)
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                Console.WriteLine($" [>] {_queueOption.ConnectionName} Received: {message}");

                var number = int.Parse(message) + 7;
                Console.WriteLine($" [...] {_queueOption.ConnectionName} working on {number}s");
                await Task.Delay(number * 1000);

                Publish(ea, channel, () =>
                {
                    Console.WriteLine($" [-] {_queueOption.ConnectionName} Sent {message}");
                    return message;
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                TagDeliveryMessage(ea, channel);
            }
        }
    }
}
