using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.OpenTelemetry.Instrumentations;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry;

public static class MonitoringExtension
{
    public static OTELBuilder AddCoreMonitor(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        var otelOptions = new OTELOption();
        configurationSection.Bind(otelOptions);
        services.Configure<OTELOption>(configurationSection);

        services.AddScoped<IAspNetCoreEnrichHttpRequest, HttpEnrichHttpRequest>();
        // services.AddScoped<IAspNetCoreEnrichHttpRequest, UserEnrichHttpRequest>();

        services.AddScoped<IAspNetCoreEnrichHttpResponse, HttpEnrichHttpResponse>();
        // services.AddScoped<IAspNetCoreEnrichHttpResponse, UserEnrichHttpResponse>();

        return new OTELBuilder(services, otelOptions);
    }
}