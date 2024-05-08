using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Jarvis.WebApi.Monitoring;

public static class MonitoringExtension
{
    public static OTLPBuilder AddCoreMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpOptions = new OTLPOption();
        var otlpSection = configuration.GetSection("OTLP");
        services.Configure<OTLPOption>(otlpSection);
        otlpSection.Bind(otlpOptions);

        return new OTLPBuilder(services, otlpOptions);
    }
}