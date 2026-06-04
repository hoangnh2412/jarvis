// Jarvis.HealthChecks — Resolve readiness probe timeout from HealthChecks:DefaultTimeoutSeconds (clamped).
using Microsoft.Extensions.Configuration;

namespace Jarvis.HealthChecks;

/// <summary>
/// Helpers for <see cref="JarvisHealthCheckOptions.DefaultTimeoutSeconds"/> when host registers readiness checks.
/// Helper cho <see cref="JarvisHealthCheckOptions.DefaultTimeoutSeconds"/> khi host đăng ký readiness.
/// </summary>
public static class HealthCheckConfigurationExtensions
{
    /// <summary>Minimum allowed readiness probe timeout (seconds).</summary>
    public const int MinTimeoutSeconds = 1;

    /// <summary>Maximum allowed readiness probe timeout (seconds).</summary>
    public const int MaxTimeoutSeconds = 120;

    /// <summary>
    /// Reads <c>HealthChecks:DefaultTimeoutSeconds</c>, clamps to <see cref="MinTimeoutSeconds"/>–<see cref="MaxTimeoutSeconds"/>.
    /// </summary>
    public static int GetDefaultTimeoutSeconds(this IConfiguration configuration, int defaultSeconds = 5) =>
        ClampTimeoutSeconds(
            configuration.GetValue(
                $"{JarvisHealthCheckOptions.SectionName}:DefaultTimeoutSeconds",
                defaultSeconds));

    /// <summary>
    /// <see cref="GetDefaultTimeoutSeconds"/> as <see cref="TimeSpan"/> for <c>IHealthChecksBuilder</c> registrations.
    /// </summary>
    public static TimeSpan GetDefaultReadinessProbeTimeout(this IConfiguration configuration, int defaultSeconds = 5) =>
        TimeSpan.FromSeconds(configuration.GetDefaultTimeoutSeconds(defaultSeconds));

    /// <summary>Clamps probe timeout seconds to the Jarvis readiness range.</summary>
    public static int ClampTimeoutSeconds(int seconds) =>
        Math.Clamp(seconds, MinTimeoutSeconds, MaxTimeoutSeconds);
}
