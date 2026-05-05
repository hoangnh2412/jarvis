using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jarvis.HealthChecks;

/// <summary>
/// Opt-in readiness registration for integration metrics (not part of <see cref="JarvisHealthCheckServiceExtensions.AddJarvisHealthChecks"/> defaults).
/// Đăng ký readiness tùy chọn cho metric tích hợp (không nằm trong mặc định của <see cref="JarvisHealthCheckServiceExtensions.AddJarvisHealthChecks"/>).
/// </summary>
public static class JarvisIntegrationMetricsHealthCheckExtensions
{
    /// <summary>
    /// Adds the <c>integration-metrics</c> readiness/detailed check using registered <see cref="IJarvisHealthIntegrationMetricsProvider"/> instances.
    /// Thêm check readiness/detailed <c>integration-metrics</c> dựa trên các <see cref="IJarvisHealthIntegrationMetricsProvider"/> đã đăng ký.
    /// </summary>
    public static IHealthChecksBuilder AddJarvisIntegrationMetricsReadinessCheck(
        this IHealthChecksBuilder healthChecks,
        TimeSpan? timeout = null)
    {
        healthChecks.Services.TryAddSingleton<IntegrationMetricsHealthCheck>();
        var t = timeout ?? TimeSpan.FromSeconds(5);
        return healthChecks.AddCheck<IntegrationMetricsHealthCheck>(
            "integration-metrics",
            failureStatus: HealthStatus.Degraded,
            tags: [HealthCheckTags.Readiness, HealthCheckTags.Integration, HealthCheckTags.Detailed],
            timeout: t);
    }
}
