// Jarvis.HealthChecks — IHealthCheck implementation for tag "startup"; no I/O, only reads IStartupCompletionNotifier.
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

        return Task.FromResult(HealthCheckResult.Unhealthy("startup_complete=false; startup_tasks_not_finished=true"));
    }
}
