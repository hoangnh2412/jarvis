using Platform.DDD.Application.Commands;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Platform.DDD.Application;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddCoreApplication(this IHostApplicationBuilder services)
    {
        services.AddCommandQuery();

        return services;
    }

    public static IHostApplicationBuilder AddCommandQuery(this IHostApplicationBuilder services)
    {
        services.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.Services.AddScoped<IAsyncCommandDispatcher, AsyncCommandDispatcher>();
        services.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.Services.AddScoped<IAsyncQueryDispatcher, AsyncQueryDispatcher>();

        return services;
    }
}
