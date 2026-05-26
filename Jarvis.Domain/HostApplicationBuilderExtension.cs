using Jarvis.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Domain;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddCoreDomain(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IWorkContext, WorkContext>();
        return builder;
    }
}