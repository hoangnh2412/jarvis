// Jarvis.HealthChecks — Background publisher invoked by HealthCheckService on a delay; logs unhealthy entries only.
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Jarvis.HealthChecks;

/// <summary>
/// <see cref="IHealthCheckPublisher"/> that writes Warning/Error logs for non-healthy entries (Azure Monitor, Loki, etc.).
/// <see cref="IHealthCheckPublisher"/> ghi log Warning/Error cho mục không healthy (Azure Monitor, Loki, v.v.).
/// </summary>
internal sealed class HealthStatusPublisher(ILogger<HealthStatusPublisher> logger)
    : IHealthCheckPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken = default)
    {
        foreach (var (name, entry) in report.Entries)
        {
            if (entry.Status == HealthStatus.Healthy)
                continue;

            // EN: Map Degraded → Warning, Unhealthy → Error / VI: Degraded → Warning, Unhealthy → Error
            var level = entry.Status == HealthStatus.Unhealthy ? LogLevel.Error : LogLevel.Warning;
            logger.Log(
                level,
                "HealthCheck {HealthCheckName} is {Status}: {Description}",
                name,
                entry.Status,
                entry.Description);

            if (entry.Exception != null)
                logger.Log(level, entry.Exception, "HealthCheck {HealthCheckName} exception", name);
        }

        return Task.CompletedTask;
    }
}
