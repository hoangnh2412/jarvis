using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Events;

namespace Jarvis.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();

        return services;
    }

}