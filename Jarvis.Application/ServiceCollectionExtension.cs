using Jarvis.Application.Commands;
using Jarvis.Application.Contracts.Commands;
using Jarvis.Application.Contracts.Queries;
using Jarvis.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        services.AddCommandQuery();

        return services;
    }

    public static IServiceCollection AddCommandQuery(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IAsyncCommandDispatcher, AsyncCommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IAsyncQueryDispatcher, AsyncQueryDispatcher>();

        return services;
    }
}
