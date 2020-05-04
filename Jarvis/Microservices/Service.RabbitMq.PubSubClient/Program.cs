using System;
using System.Threading.Tasks;
using Infrastructure.Message.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Service.RabbitMq.PubSubClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var services = ServiceConfigure(args);

            var testService = services.GetService<ITestService>();
            await testService.PublishAsync(args[0]);
            Console.WriteLine("Done");
        }

        private static ServiceProvider ServiceConfigure(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<RabbitOption>(configuration.GetSection("RabbitMq"));
            services.AddSingleton<ITestService, TestService>(serviceProvider =>
            {
                var rabbitOption = serviceProvider.GetService<IOptions<RabbitOption>>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var rabbitQueueOption = configuration.GetSection("RabbitMq:Queues:Test").Get<RabbitQueueOption>();
                return new TestService(rabbitQueueOption, rabbitOption);
            });
            return services.BuildServiceProvider();
        }
    }
}
