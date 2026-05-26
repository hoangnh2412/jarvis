// ReadinessHealthCheckExtensions.cs — host-owned readiness registration
// Replace {App} with your application namespace.

using Jarvis.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace {App}.Health;

public static class {App}ReadinessHealthCheckExtensions
{
    private const string ReadinessSection = "HealthChecks:Readiness";

    public static IHostApplicationBuilder Add{App}ReadinessHealthChecks(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var readiness = configuration.GetSection(ReadinessSection);

        var timeoutSeconds = Math.Clamp(configuration.GetValue("HealthChecks:DefaultTimeoutSeconds", 5), 1, 120);
        var probeTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        var healthChecks = builder.Services.AddHealthChecks();

        // Add TryAdd*Readiness methods — snippets from providers/*/SKILL.md
        // TryAddNpgSqlReadiness(healthChecks, configuration, readiness, probeTimeout);

        return builder;
    }

    // Example: custom HTTP readiness (see providers/http/SKILL.md)
    // builder.Services.TryAddSingleton<MyApiHealthCheck>();
    // healthChecks.AddCheck<MyApiHealthCheck>("my-api", failureStatus: HealthStatus.Unhealthy,
    //     tags: [HealthCheckTags.Readiness], timeout: probeTimeout);
}

// MyApiHealthCheck.cs — custom IHealthCheck template
/*
using Microsoft.Extensions.Diagnostics.HealthChecks;

public sealed class MyApiHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HttpClientName = "MyApi";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync("https://example/health", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Endpoint returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Endpoint unreachable", ex);
        }
    }
}
*/
