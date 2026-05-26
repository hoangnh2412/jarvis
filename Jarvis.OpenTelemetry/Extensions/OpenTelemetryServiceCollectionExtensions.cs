using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Configuration;
using Jarvis.OpenTelemetry.Hosting;
using Jarvis.OpenTelemetry.Instrumentations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Extensions;

public static class OpenTelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Registers Jarvis OpenTelemetry services and options from configuration section <c>OTEL</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="configureServices">Optional callback to register enrichers, plug-ins (<see cref="Abstractions.ITraceInstrumentation"/>, <see cref="Abstractions.ITraceExporter"/>, etc.), and other services.</param>
    /// <returns>A fluent builder for resource, trace, metrics, and logging.</returns>
    public static JarvisOpenTelemetryHostBuilder AddJarvisOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection>? configureServices = null)
    {
        var configurationSection = configuration.GetSection("OTEL");
        services.Configure<JarvisOpenTelemetryOptions>(configurationSection);

        var options = configurationSection.Get<JarvisOpenTelemetryOptions>() ?? new JarvisOpenTelemetryOptions();
        var builder = new JarvisOpenTelemetryHostBuilder(services, options, configurationSection);

        services.AddSingleton<IAspNetCoreEnrichHttpRequest, HttpRequestHeaderEnrichment>();
        services.AddSingleton<IAspNetCoreEnrichHttpRequest, UserRequestEnrichment>();

        services.AddSingleton<IAspNetCoreEnrichHttpResponse, HttpResponseHeaderEnrichment>();
        services.AddSingleton<IAspNetCoreEnrichHttpResponse, UserResponseEnrichment>();

        configureServices?.Invoke(services);
        return builder;
    }
}
