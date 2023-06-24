using Infrastructure.Message.Rabbit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Message.Rabbit
{
    public static class ServiceCollectionExtension
    {
        public static void AddRabbitMQ(this IServiceCollection services, RabbitOption options)
        {
            services.Configure<RabbitOption>(config =>
            {
                config.Hosts = options.Hosts;
                config.Password = options.Password;
                config.UserName = options.UserName;
                config.VirtualHost = options.VirtualHost;
            });
            services.AddSingleton<IEventBusConnector, EventBusConnector>();
            services.AddSingleton<IBusService, BusService>();
        }
    }
}