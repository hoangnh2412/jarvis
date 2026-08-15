namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Computes the next UTC occurrence for a cron expression.
/// </summary>
public interface ICronSchedule
{
    DateTimeOffset? GetNextOccurrence(DateTimeOffset fromUtc, TimeZoneInfo timeZone);
}
