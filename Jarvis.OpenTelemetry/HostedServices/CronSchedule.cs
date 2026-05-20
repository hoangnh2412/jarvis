using Cronos;

namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Cron schedule backed by <see href="https://github.com/HangfireIO/Cronos">Cronos</see> (standard 5-field cron).
/// </summary>
public sealed class CronSchedule : ICronSchedule
{
    private readonly CronExpression _expression;

    public CronSchedule(string cronExpression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cronExpression);
        _expression = CronExpression.Parse(cronExpression, CronFormat.Standard);
    }

    public DateTimeOffset? GetNextOccurrence(DateTimeOffset fromUtc, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        return _expression.GetNextOccurrence(fromUtc, timeZone);
    }
}
