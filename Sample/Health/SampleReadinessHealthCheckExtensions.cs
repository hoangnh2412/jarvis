using Jarvis.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Sample.Health;

/// <summary>
/// Sample readiness checks. Connection strings resolve from <c>HealthChecks:Readiness</c> using
/// <b>full configuration key paths</b> (colon-separated), e.g. <c>ConnectionStrings:MasterDbContext</c> or <c>Cache:DistributedGroups:Redis:Default:Configuration</c>.
/// </summary>
public static class SampleReadinessHealthCheckExtensions
{
    private const string ReadinessSection = "HealthChecks:Readiness";

    /// <summary>
    /// Appends readiness checks to the shared <see cref="IHealthChecksBuilder"/> pipeline. Call after
    /// <c>builder.AddHealthChecks()</c> from Jarvis.HealthChecks. Uses <c>HealthChecks:Readiness</c> for config key paths and
    /// registers typed HTTP checks for sample external APIs.
    /// </summary>
    /// <param name="builder">The web application builder (configuration + services).</param>
    /// <returns>The same builder for chaining.</returns>
    public static IHostApplicationBuilder AddSampleReadinessHealthChecks(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var readiness = configuration.GetSection(ReadinessSection);

        var timeoutSeconds = Math.Clamp(configuration.GetValue("HealthChecks:DefaultTimeoutSeconds", 5), 1, 120);
        var probeTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        var healthChecks = builder.Services.AddHealthChecks();

        TryAddNpgSqlReadiness(healthChecks, configuration, readiness, probeTimeout);
        TryAddRedisReadiness(healthChecks, configuration, readiness, probeTimeout);

        builder.Services.TryAddSingleton<SampleDogCeoApiHealthCheck>();
        builder.Services.TryAddSingleton<SampleArticArtworksApiHealthCheck>();
        healthChecks
            .AddCheck<SampleDogCeoApiHealthCheck>(
                "dog-ceo-api",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Readiness],
                timeout: probeTimeout)
            .AddCheck<SampleArticArtworksApiHealthCheck>(
                "artic-artworks-api",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Readiness],
                timeout: probeTimeout);

        return builder;
    }

    /// <summary>
    /// Reads <c>Readiness:Database</c> as a configuration key path and uses <see cref="IConfiguration.this[string]"/> to obtain the PostgreSQL connection string.
    /// </summary>
    private static void TryAddNpgSqlReadiness(
        IHealthChecksBuilder healthChecks,
        IConfiguration configuration,
        IConfigurationSection readiness,
        TimeSpan probeTimeout)
    {
        var keyPath = readiness.GetValue<string>("Database");
        if (string.IsNullOrWhiteSpace(keyPath))
            return;

        var connectionString = configuration[keyPath.Trim()];
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        healthChecks.AddNpgSql(
            connectionString,
            name: "postgresql",
            failureStatus: HealthStatus.Unhealthy,
            tags: [HealthCheckTags.Readiness],
            timeout: probeTimeout);
    }

    /// <summary>
    /// Reads <c>Readiness:Redis</c> as a configuration key path and resolves the Redis connection/configuration value the same way.
    /// </summary>
    private static void TryAddRedisReadiness(
        IHealthChecksBuilder healthChecks,
        IConfiguration configuration,
        IConfigurationSection readiness,
        TimeSpan probeTimeout)
    {
        var keyPath = readiness.GetValue<string>("Redis");
        if (string.IsNullOrWhiteSpace(keyPath))
            return;

        var redisConnection = configuration[keyPath.Trim()];
        if (string.IsNullOrWhiteSpace(redisConnection))
            return;

        healthChecks.AddRedis(
            redisConnection,
            name: "redis",
            failureStatus: HealthStatus.Unhealthy,
            tags: [HealthCheckTags.Readiness],
            timeout: probeTimeout);
    }
}
