using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jarvis.HealthChecks;

/// <summary>
/// Startup probe: unhealthy until <see cref="IStartupCompletionNotifier.MarkStartupComplete"/> runs; no external I/O.
/// Probe startup: unhealthy cho đến khi chạy <see cref="IStartupCompletionNotifier.MarkStartupComplete"/>; không gọi mạng.
/// </summary>
internal sealed class StartupCompletionHealthCheck(IStartupCompletionNotifier notifier)
    : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (notifier.IsStartupComplete)
            return Task.FromResult(HealthCheckResult.Healthy("startup_complete=true"));

        return Task.FromResult(
            HealthCheckResult.Unhealthy(
                "Startup tasks not finished (migrations / warm-up). Delay startupProbe initialDelaySeconds ~30 - 60s in Kubernetes. / " +
                "Tác vụ startup chưa xong (migration / warm-up). Nên đặt startupProbe initialDelaySeconds ~30 - 60s trên Kubernetes."));
    }
}
