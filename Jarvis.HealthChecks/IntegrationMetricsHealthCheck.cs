using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jarvis.HealthChecks;

/// <summary>
/// Concatenates all registered <see cref="IJarvisHealthIntegrationMetricsProvider"/> outputs for readiness/detailed JSON.
/// Nối kết quả mọi <see cref="IJarvisHealthIntegrationMetricsProvider"/> đã đăng ký cho JSON readiness/chi tiết.
/// </summary>
internal sealed class IntegrationMetricsHealthCheck(IEnumerable<IJarvisHealthIntegrationMetricsProvider> providers)
    : IHealthCheck
{
    private readonly IJarvisHealthIntegrationMetricsProvider[] _providers = providers.ToArray();

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        if (_providers.Length == 0)
        {
            return HealthCheckResult.Healthy(
                $"integration_metrics=none; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1} / " +
                $"integration_metrics=không_có; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
        }

        var parts = new List<string>(_providers.Length);
        foreach (var p in _providers)
            parts.Add(await p.GetMetricsDescriptionAsync(cancellationToken).ConfigureAwait(false));

        var description = string.Join("; ", parts) + $"; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}";
        return HealthCheckResult.Healthy(description);
    }
}
