using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Jarvis.OpenTelemetry.Instrumentations;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry;

public static class MonitoringExtension
{
    public static OTELBuilder AddCoreMonitor(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection>? configService = null)
    {
        var configurationSection = configuration.GetSection("OTEL");
        services.Configure<OTELOption>(configurationSection);

        var otelOptions = configurationSection.Get<OTELOption>() ?? new OTELOption();
        var builder = new OTELBuilder(services, otelOptions);

        services.AddScoped<IAspNetCoreEnrichHttpRequest, HttpEnrichHttpRequest>();
        services.AddScoped<IAspNetCoreEnrichHttpRequest, UserEnrichHttpRequest>();

        services.AddScoped<IAspNetCoreEnrichHttpResponse, HttpEnrichHttpResponse>();
        services.AddScoped<IAspNetCoreEnrichHttpResponse, UserEnrichHttpResponse>();

        configService?.Invoke(services);
        return builder;
    }
}