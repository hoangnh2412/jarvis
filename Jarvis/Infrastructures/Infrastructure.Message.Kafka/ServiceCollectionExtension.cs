using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Message.Kafka
{
    public static class ServiceCollectionExtension
    {
        public static void AddKafka(this IServiceCollection services)
        {
            services.AddSingleton<IKafkaClient, KafkaClient>();
        }
    }
}