// Jarvis.HealthChecks — HTTP mapping: Kubernetes-style probe routes, detailed /health, optional API key gate, UI SPA, Prometheus middleware.
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jarvis.HealthChecks;

/// <summary>
/// Minimal-hosting endpoint mapping helpers for health routes and Prometheus exporter middleware.
/// Helper map endpoint cho minimal hosting: route health và middleware Prometheus.
/// </summary>
public static class HealthCheckWebApplicationExtensions
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
    /// When <see cref="JarvisHealthCheckOptions.MarkStartupCompleteOnApplicationStarted"/> is <c>true</c> (default), startup completion is signaled on <see cref="IHostApplicationLifetime.ApplicationStarted"/>.
    /// </remarks>
    /// <returns>The same <paramref name="app"/> for fluent chaining.</returns>
    public static WebApplication UseHealthChecks(this WebApplication app)
    {
        var opts = app.Services.GetRequiredService<IOptions<JarvisHealthCheckOptions>>().Value;

        if (opts.EnablePrometheusMetrics)
            app.UseHealthChecksPrometheusExporter(opts.PrometheusMetricsPath);

        // EN: Liveness — detailed JSON / VI: Liveness — JSON chi tiết
        app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Liveness),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Readiness — detailed JSON / VI: Readiness — JSON chi tiết
        app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Readiness),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Startup — detailed JSON / VI: Startup — JSON chi tiết
        app.MapHealthChecks(
                "/health/startup",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains(HealthCheckTags.Startup),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    AllowCachingResponses = false,
                })
            .DisableRateLimiting();

        // EN: Detailed UI-compatible JSON / VI: JSON chi tiết tương thích UI.Client
        var detailed = app.MapHealthChecks(
            "/health",
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
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

        if (opts.MarkStartupCompleteOnApplicationStarted)
        {
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                app.Services.GetRequiredService<IStartupCompletionNotifier>().MarkStartupComplete();
            });
        }

        return app;
    }

    /// <summary>
    /// Adds Prometheus pull exporter middleware for health metrics (parallel to OTLP metrics elsewhere).
    /// Idempotent with respect to <see cref="UseHealthChecks"/> when both enable Prometheus — either path is safe.
    /// </summary>
    /// <remarks>
    /// Call after <c>WebApplication</c> is built; honors <see cref="JarvisHealthCheckOptions.EnablePrometheusMetrics"/> and <see cref="JarvisHealthCheckOptions.PrometheusMetricsPath"/>.
    /// </remarks>
    /// <returns>The same <paramref name="app"/> for fluent chaining.</returns>
    public static WebApplication UseJarvisHealthChecksPrometheusExporter(this WebApplication app)
    {
        var opts = app.Services.GetRequiredService<IOptions<JarvisHealthCheckOptions>>().Value;
        if (opts.EnablePrometheusMetrics)
            app.UseHealthChecksPrometheusExporter(opts.PrometheusMetricsPath);
        return app;
    }
}
