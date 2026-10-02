using Platform.OpenTelemetry.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Platform.OpenTelemetry.Extensions;

public static class OpenTelemetryApplicationBuilderExtensions
{
    /// <summary>
    /// Adds trace and log enrichment middleware. Call after routing if enrichers depend on endpoint.
    /// </summary>
    public static IApplicationBuilder UsePlatformOpenTelemetry(this IApplicationBuilder app)
    {
        app.UseMiddleware<TraceEnrichmentMiddleware>();
        app.UseMiddleware<LogEnrichmentMiddleware>();
        return app;
    }
}
