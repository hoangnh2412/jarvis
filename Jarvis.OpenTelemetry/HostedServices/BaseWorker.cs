using System.Diagnostics;
using System.Reflection;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Cron-driven background worker. Each tick runs in its own trace and log scope.
/// Transport-agnostic — subclass implements <see cref="ExecuteJobAsync"/>.
/// </summary>
public abstract class BaseWorker(
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    /// <summary>Standard 5-field cron expression (e.g. <c>0 */5 * * *</c>).</summary>
    protected abstract string CronExpression { get; }

    /// <summary>Logical job name used in spans, logs, and metrics.</summary>
    protected virtual string JobName => GetType().Name;

    /// <summary>Must match <c>AddSource</c> in Jarvis OpenTelemetry trace configuration.</summary>
    protected virtual string ActivitySourceName =>
        Assembly.GetEntryAssembly()?.GetName().Name ?? typeof(BaseWorker).Assembly.GetName().Name!;

    protected virtual TimeZoneInfo ScheduleTimeZone => TimeZoneInfo.Utc;

    protected virtual ICronSchedule CreateCronSchedule() => new CronSchedule(CronExpression);

    /// <summary>Job body executed once per cron tick inside a DI scope.</summary>
    protected abstract Task ExecuteJobAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

    /// <summary>Override to add tags or log scope fields for a tick.</summary>
    protected virtual void ConfigureTelemetry(
        Activity? activity,
        Dictionary<string, object?> logScope)
    {
        activity?.SetTag(TelemetryContextAttributes.WorkerName, JobName);
        logScope[TelemetryContextAttributes.WorkerName] = JobName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedule = CreateCronSchedule();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = schedule.GetNextOccurrence(now, ScheduleTimeZone);
            if (next is null)
            {
                _logger.LogWarning("Cron schedule for worker {WorkerName} returned no next occurrence", JobName);
                break;
            }

            var delay = next.Value - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);

            if (stoppingToken.IsCancellationRequested)
                break;

            await RunTickAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task RunTickAsync(CancellationToken cancellationToken)
    {
        var operationName = $"{JobName}.execute";
        var success = false;

        try
        {
            await HostedServiceTelemetry.RunAsync(
                _scopeFactory,
                _logger,
                ActivitySourceName,
                operationName,
                ActivityKind.Internal,
                messageContext: null,
                executeAsync: async (serviceProvider, _, ct) =>
                {
                    await ExecuteJobAsync(serviceProvider, ct).ConfigureAwait(false);
                },
                configureLogScope: ConfigureTelemetry,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerName} tick failed", JobName);
        }
        finally
        {
            HostedServiceTelemetryMetrics.RecordWorkerExecution(JobName, success);
        }
    }
}
