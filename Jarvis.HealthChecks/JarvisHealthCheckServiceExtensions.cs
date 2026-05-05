using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Jarvis.HealthChecks;

/// <summary>
/// Dependency-injection helpers registering Jarvis liveness and startup probes plus optional HealthChecks UI.
/// Phương thức mở rộng DI đăng ký probe liveness và startup Jarvis cùng HealthChecks UI tùy chọn.
/// </summary>
/// <remarks>
/// Core registers only liveness + startup; readiness (SQL, Redis, DNS, disk, HTTP deps, integration metrics) is host-owned.
/// See <see cref="JarvisIntegrationMetricsHealthCheckExtensions.AddJarvisIntegrationMetricsReadinessCheck"/> for optional integration metrics.
/// Lõi chỉ đăng ký liveness + startup; readiness (SQL, Redis, DNS, đĩa, HTTP, integration metrics) do host tự đăng ký.
/// </remarks>
public static class JarvisHealthCheckServiceExtensions
{
    /// <summary>
    /// Registers liveness + startup health checks, optional UI + webhooks + InMemory storage, and log publisher.
    /// Đăng ký health check liveness + startup, UI tùy chọn + webhook + InMemory, và publisher log.
    /// </summary>
    /// <param name="builder">EN: Host builder / VI: Builder của host.</param>
    /// <param name="configure">EN: Optional post-bind mutation / VI: Chỉnh sửa tùy chọn sau khi bind config.</param>
    public static IHostApplicationBuilder AddJarvisHealthChecks(this IHostApplicationBuilder builder, Action<JarvisHealthCheckOptions>? configure = null)
    {
        builder.Services
            .AddOptions<JarvisHealthCheckOptions>()
            .BindConfiguration(JarvisHealthCheckOptions.SectionName);

        if (configure != null)
            builder.Services.Configure(configure);

        var options = new JarvisHealthCheckOptions();
        builder.Configuration.GetSection(JarvisHealthCheckOptions.SectionName).Bind(options);
        configure?.Invoke(options);

        builder.Services.AddMemoryCache();
        builder.Services.TryAddSingleton<IStartupCompletionNotifier, StartupCompletionNotifier>();
        builder.Services.AddSingleton<ProcessResourceLivenessHealthCheck>();
        builder.Services.AddSingleton<StartupCompletionHealthCheck>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, JarvisHealthStatusPublisher>());
        builder.Services.Configure<HealthCheckPublisherOptions>(o =>
        {
            if (o.Delay == default)
                o.Delay = TimeSpan.FromSeconds(10);
        });

        var healthChecks = builder.Services.AddHealthChecks()
            .AddCheck<ProcessResourceLivenessHealthCheck>(
                "process-resources",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Liveness],
                timeout: TimeSpan.FromMilliseconds(800))
            .AddCheck<StartupCompletionHealthCheck>(
                "startup-completion",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Startup],
                timeout: TimeSpan.FromSeconds(2));

        if (options.ProcessAllocatedMemoryMegabytesCeiling > 0)
        {
            healthChecks.AddProcessAllocatedMemoryHealthCheck(
                options.ProcessAllocatedMemoryMegabytesCeiling,
                name: "process-allocated-memory",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Liveness],
                timeout: TimeSpan.FromMilliseconds(500));
        }

        if (options.Ui.Enabled)
            JarvisHealthChecksThirdPartyUi.HealthChecksUiThirdPartyRegistration.RegisterHealthChecksUi(builder.Services, options);

        return builder;
    }
}
