// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using Infrastructure.Message.Rabbit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;

// namespace Service.RabbitMq.PubSub
// {
//     public class WorkerPreProcess : RabbitClient<string, string>
//     {
//         public WorkerPreProcess(IConfiguration configuration, IOptions<RabbitOption> rabbitOptions) : base(configuration, rabbitOptions)
//         {
//             InitChannel(configuration, "PreProcess");
//             BasicQos();
//             InitQueue(queueName: "preprocess");
//             InitInput(exchangeName: "preprocess", routingKeys: new List<string> { "#" });
//             InitOutput(exchangeName: "relax");
//         }

//         public override async Task HandleAsync(BasicDeliverEventArgs ea, string message)
//         {
//             try
//             {
//                 Console.WriteLine($" [>] {_queueOptions.ConnectionName} Received: {message}");

//                 var number = int.Parse(message);
//                 Console.WriteLine($" [...] {_queueOptions.ConnectionName} working on {number}s");
//                 await Task.Delay(number * 1000);

//                 Publish(message, "relax", "");
//             }
//             catch (Exception ex)
//             {
//                 throw ex;
//             }
//             finally
//             {
//                 BasicAck(ea);
//             }
//         }
//     }
// }
