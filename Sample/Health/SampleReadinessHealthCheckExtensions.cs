using Jarvis.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.Health;

/// <summary>
/// Sample-only readiness checks (not part of Jarvis.HealthChecks Core defaults).
/// </summary>
public static class SampleReadinessHealthCheckExtensions
{
    private const string SampleSection = "Sample:ReadinessHealthChecks";

    public static IHostApplicationBuilder AddSampleReadinessHealthChecks(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var sample = configuration.GetSection(SampleSection);
        var pgConnectionStringName = sample.GetValue<string>("NpgsqlConnectionStringName");
        var redisConnectionString = sample.GetValue<string>("RedisConnectionString");

        var timeoutSeconds = sample.GetValue("DependencyTimeoutSeconds", 5);
        var dependencyTimeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 1, 120));

        var healthChecks = builder.Services.AddHealthChecks();

        healthChecks.AddJarvisIntegrationMetricsReadinessCheck(dependencyTimeout);

        if (!string.IsNullOrWhiteSpace(pgConnectionStringName))
        {
            var name = pgConnectionStringName!;
            healthChecks.AddNpgSql(
                sp => sp.GetRequiredService<IConfiguration>().GetConnectionString(name) ?? "",
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Readiness],
                timeout: dependencyTimeout);
        }

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(
                redisConnectionString!,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Readiness],
                timeout: dependencyTimeout);
        }

        var diskPaths = sample.GetSection("DiskPaths").Get<List<SampleReadinessDiskPathOptions>>() ?? [];
        foreach (var disk in diskPaths)
        {
            if (string.IsNullOrWhiteSpace(disk.Path))
                continue;
            var path = disk.Path;
            healthChecks.AddDiskStorageHealthCheck(
                o => o.AddDrive(path, disk.MinimumFreeMegabytes),
                name: $"disk:{path}",
                failureStatus: HealthStatus.Degraded,
                tags: [HealthCheckTags.Readiness],
                timeout: TimeSpan.FromSeconds(2));
        }

        return builder;
    }
}
