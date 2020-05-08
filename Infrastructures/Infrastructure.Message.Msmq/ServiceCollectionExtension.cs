using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Message.Msmq
{
    public static class ServiceCollectionExtension
    {
        public static void AddMsmq(this IServiceCollection services)
        {
            services.AddSingleton<IMsmqClient, MsmqClient>();
        }
    }
}