using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Events;
using Jarvis.Shared.DependencyInjection;

namespace Jarvis.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddScoped(typeof(IServiceFactory<>), typeof(ServiceFactory<>));

        return services;
    }

}