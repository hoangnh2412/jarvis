using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Service.RabbitMq.PubSub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<RabbitOption>(hostContext.Configuration.GetSection("RabbitMq"));
                    // AddWorker<WorkerPreProcess>(services, "RabbitMq:Workers:PreProcess");
                    // AddWorker<WorkerEat>(services, "RabbitMq:Workers:Eat");
                    // AddWorker<WorkerCode>(services, "RabbitMq:Workers:Code");
                    // AddWorker<WorkerCodeHotfix>(services, "RabbitMq:Workers:CodeHotfix");
                    // AddWorker<WorkerCodeRelease>(services, "RabbitMq:Workers:CodeRelease");
                });

        private static void AddWorker<T>(IServiceCollection services, string name) where T : class, IHostedService
        {
            services.AddHostedService(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var rabbitOption = serviceProvider.GetService<IOptions<RabbitOption>>();

                return (T)Activator.CreateInstance(typeof(T), new object[] { name, configuration, rabbitOption });
            });
        }
    }
}
