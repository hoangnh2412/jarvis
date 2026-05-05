using HealthChecks.Prometheus.Metrics;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Jarvis.HealthChecks;

/// <summary>
/// Minimal-hosting endpoint mapping helpers for health routes and Prometheus exporter middleware.
/// Helper map endpoint cho minimal hosting: route health và middleware Prometheus.
/// </summary>
public static class JarvisHealthCheckWebApplicationExtensions
{
    /// <summary>
    /// Maps <c>/health/live</c>, <c>/health/ready</c> (host-registered readiness checks), <c>/health/startup</c>, detailed <c>/health</c>, optional UI SPA.
    /// Register Prometheus via <see cref="UseJarvisHealthChecksPrometheusExporter"/>.
    /// Map <c>/health/live</c>, <c>/health/ready</c> (check readiness do host đăng ký), <c>/health/startup</c>, <c>/health</c> chi tiết, SPA UI tùy chọn.
    /// Đăng ký Prometheus qua <see cref="UseJarvisHealthChecksPrometheusExporter"/>.
    /// </summary>
    /// <remarks>
    /// Kubernetes tip: HTTP probes, failureThreshold 3, successThreshold 1, startup initialDelay ~60s.
    /// gRPC health requires a separate gRPC health service package if desired.
    /// Gợi ý Kubernetes: probe HTTP, failureThreshold 3, successThreshold 1, startup initialDelay ~60s.
    /// gRPC health cần gói dịch vụ health gRPC riêng nếu dùng.
    /// </remarks>
    public static WebApplication MapJarvisHealthCheckEndpoints(this WebApplication app)
    {
        var opts = app.Services.GetRequiredService<IOptions<JarvisHealthCheckOptions>>().Value;

        // EN: Liveness — minimal JSON / VI: Liveness — JSON tối giản
        app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Liveness),
                    ResponseWriter = WriteMinimalJsonAsync,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Readiness — minimal JSON / VI: Readiness — JSON tối giản
        app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Readiness),
                    ResponseWriter = WriteMinimalJsonAsync,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Startup — minimal JSON / VI: Startup — JSON tối giản
        app.MapHealthChecks(
                "/health/startup",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Startup),
                    ResponseWriter = WriteMinimalJsonAsync,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Detailed UI-compatible JSON / VI: JSON chi tiết tương thích UI.Client
        var detailed = app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

        // EN: Optional API key gate for detailed endpoint / VI: Khóa API tùy chọn cho endpoint chi tiết
        if (!string.IsNullOrEmpty(opts.DetailedEndpointApiKey))
        {
            var headerName = opts.DetailedEndpointApiKeyHeader;
            var expected = opts.DetailedEndpointApiKey;
            detailed.AddEndpointFilter(async (context, next) =>
            {
                if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var provided) ||
                    provided.Count != 1 ||
                    !string.Equals(provided[0], expected, StringComparison.Ordinal))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Results.Empty;
                }

                return await next(context);
            });
        }

        // EN: HealthChecks UI SPA + API static hosting / VI: Host SPA UI + API tĩnh của HealthChecks
        if (opts.Ui.Enabled)
        {
            app.MapHealthChecksUI(options =>
            {
                options.UIPath = opts.Ui.UIPath.TrimEnd('/');
                options.ApiPath = opts.Ui.ApiPath.TrimEnd('/');
            });
        }

        return app;
    }

    /// <summary>
    /// Adds Prometheus pull exporter middleware for health metrics (parallel to OTLP metrics elsewhere).
    /// Thêm middleware exporter Prometheus (kéo metrics) song song với OTLP nơi khác nếu có.
    /// </summary>
    public static WebApplication UseJarvisHealthChecksPrometheusExporter(this WebApplication app)
    {
        var opts = app.Services.GetRequiredService<IOptions<JarvisHealthCheckOptions>>().Value;
        if (opts.EnablePrometheusMetrics)
            app.UseHealthChecksPrometheusExporter(opts.PrometheusMetricsPath);
        return app;
    }

    /// <summary>
    /// EN: Writes {"status":"Healthy|Degraded|Unhealthy"} for public probes / VI: Ghi JSON trạng thái tối giản cho probe công khai.
    /// </summary>
    private static Task WriteMinimalJsonAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        return httpContext.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
    }
}
