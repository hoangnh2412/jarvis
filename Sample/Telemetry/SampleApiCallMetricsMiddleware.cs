using System.Diagnostics.Metrics;

namespace Sample.Telemetry;

/// <summary>
/// Custom metrics: counts HTTP calls to Users v1, Users v2, and Roles APIs.
/// The meter name matches <c>AddMeter(Assembly.GetEntryAssembly().Name)</c> in Jarvis OpenTelemetry defaults.
/// </summary>
public sealed class SampleApiCallMetricsMiddleware(RequestDelegate next)
{
    private static readonly Meter Meter = new("Sample", "1.0.0");
    private static readonly Counter<long> ApiCalls = Meter.CreateCounter<long>(
        "sample.http.api.calls",
        unit: "{request}",
        description: "Number of requests to Sample demo API surfaces (users v1/v2, roles).");

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        var group = ClassifyApiGroup(context.Request.Path);
        if (group is not null)
            ApiCalls.Add(1, new KeyValuePair<string, object?>("api", group));
    }

    private static string? ClassifyApiGroup(PathString path)
    {
        var p = path.Value?.ToLowerInvariant() ?? string.Empty;
        if (p.Contains("/users", StringComparison.Ordinal))
        {
            if (p.Contains("/v1", StringComparison.Ordinal))
                return "users_v1";
            if (p.Contains("/v2", StringComparison.Ordinal))
                return "users_v2";
        }

        if (p.Contains("/roles", StringComparison.Ordinal))
            return "roles";

        return null;
    }
}
