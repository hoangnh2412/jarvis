using Jarvis.OpenTelemetry.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Jarvis.OpenTelemetry.Extensions;

public static class OpenTelemetryApplicationBuilderExtensions
{
    /// <summary>
    /// Adds trace and log enrichment middleware. Call after routing if enrichers depend on endpoint.
    /// </summary>
    public static IApplicationBuilder UseJarvisOpenTelemetry(this IApplicationBuilder app)
    {
        app.UseMiddleware<TraceEnrichmentMiddleware>();
        app.UseMiddleware<LogEnrichmentMiddleware>();
        return app;
    }
}
