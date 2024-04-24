using Jarvis.Infrastructure.DistributedEvent.RabbitMQ;

namespace Sample.EventBus;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new RabbitMQOption();
        configuration.GetSection("RabbitMq").Bind(options);
        services.Configure<RabbitMQOption>(configuration.GetSection("RabbitMq"));

        services.Configure<RabbitMQOption>(config =>
        {
            config.Hosts = options.Hosts;
            config.Password = options.Password;
            config.UserName = options.UserName;
            config.VirtualHost = options.VirtualHost;
        });
        services.AddSingleton<IRabbitMQConnector, RabbitMQConnector>();
        return services;
    }
}