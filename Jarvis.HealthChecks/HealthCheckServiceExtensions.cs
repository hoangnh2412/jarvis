// Jarvis.HealthChecks — Core DI: binds HealthChecks options, registers liveness + startup IHealthCheck entries,
// optional AspNetCore.HealthChecks.System probes, HealthChecks UI (InMemory), and IHealthCheckPublisher for logs.
using System.Diagnostics;
using System.ServiceProcess;
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
/// Core registers liveness, startup, and optional CLR allocated-memory liveness.
/// Readiness (SQL, Redis, DNS, disk, HTTP deps, …) is host-owned via <see cref="IHealthChecksBuilder"/> extensions.
/// Lõi đăng ký liveness, startup và liveness bộ nhớ CLR tùy chọn.
/// Readiness (SQL, Redis, DNS, đĩa, HTTP, …) do host đăng ký qua extension <see cref="IHealthChecksBuilder"/>.
/// </remarks>
public static class HealthCheckServiceExtensions
{
    /// <summary>
    /// Registers liveness + startup health checks, optional UI + webhooks + InMemory storage, and log publisher.
    /// Đăng ký health check liveness + startup, UI tùy chọn + webhook + InMemory, và publisher log.
    /// </summary>
    /// <param name="builder">EN: Host builder / VI: Builder của host.</param>
    /// <param name="configure">EN: Optional post-bind mutation / VI: Chỉnh sửa tùy chọn sau khi bind config.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static IHostApplicationBuilder AddHealthChecks(this IHostApplicationBuilder builder, Action<JarvisHealthCheckOptions>? configure = null)
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

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, HealthStatusPublisher>());
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

        RegisterAspNetCoreSystemHealthChecks(healthChecks, options);

        if (options.Ui.Enabled)
            JarvisHealthChecksThirdPartyUi.HealthChecksUiThirdPartyRegistration.RegisterHealthChecksUi(builder.Services, options);

        return builder;
    }

    /// <summary>
    /// Registers optional liveness checks from the <c>AspNetCore.HealthChecks.System</c> package when limits or paths in
    /// <see cref="JarvisHealthCheckOptions.System"/> are set (non-zero sizes, non-empty drive/folder/file lists, etc.).
    /// </summary>
    /// <param name="healthChecks">The same <see cref="IHealthChecksBuilder"/> chain as Core checks (shared pipeline).</param>
    /// <param name="options">Snapshot after configuration bind; drives which System checks are added.</param>
    private static void RegisterAspNetCoreSystemHealthChecks(IHealthChecksBuilder healthChecks, JarvisHealthCheckOptions options)
    {
        var sys = options.System;
        var livenessTags = new[] { HealthCheckTags.Liveness };
        var timeout = TimeSpan.FromMilliseconds(500);

        if (options.ProcessAllocatedMemoryMegabytesCeiling > 0)
        {
            healthChecks.AddProcessAllocatedMemoryHealthCheck(
                options.ProcessAllocatedMemoryMegabytesCeiling,
                name: "process-allocated-memory",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        if (sys.PrivateMemoryMegabytesMaximum > 0)
        {
            healthChecks.AddPrivateMemoryHealthCheck(
                sys.PrivateMemoryMegabytesMaximum * 1024L * 1024L,
                name: "system-private-memory",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        if (sys.WorkingSetMegabytesMaximum > 0)
        {
            healthChecks.AddWorkingSetHealthCheck(
                sys.WorkingSetMegabytesMaximum * 1024L * 1024L,
                name: "system-working-set",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        if (sys.VirtualMemorySizeMegabytesMaximum > 0)
        {
            healthChecks.AddVirtualMemorySizeHealthCheck(
                sys.VirtualMemorySizeMegabytesMaximum * 1024L * 1024L,
                name: "system-virtual-memory-size",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        var diskDrives = sys.DiskDrives.Where(static d => !string.IsNullOrWhiteSpace(d.Path)).ToList();
        if (diskDrives.Count > 0)
        {
            healthChecks.AddDiskStorageHealthCheck(
                setup: o =>
                {
                    foreach (var d in diskDrives)
                        o.AddDrive(d.Path.Trim(), d.MinimumFreeMegabytes);
                },
                name: "system-disk-storage",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        var folders = sys.MonitorFolders.Where(static p => !string.IsNullOrWhiteSpace(p)).Select(static p => p.Trim()).ToList();
        if (folders.Count > 0)
        {
            healthChecks.AddFolder(
                setup: o =>
                {
                    o.CheckAllFolders = sys.FolderCheckAll;
                    foreach (var p in folders)
                        o.Folders.Add(p);
                },
                name: "system-folder",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        var files = sys.MonitorFiles.Where(static p => !string.IsNullOrWhiteSpace(p)).Select(static p => p.Trim()).ToList();
        if (files.Count > 0)
        {
            healthChecks.AddFile(
                setup: o =>
                {
                    o.CheckAllFiles = sys.FileCheckAll;
                    foreach (var p in files)
                        o.Files.Add(p);
                },
                name: "system-file",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        // EN: null = current process; explicit "" disables / VI: null = tiến trình hiện tại; "" = tắt
        if (sys.ProcessName is not null && string.IsNullOrWhiteSpace(sys.ProcessName))
        {
            // disabled explicitly
        }
        else
        {
            var processName = string.IsNullOrWhiteSpace(sys.ProcessName)
                ? Process.GetCurrentProcess().ProcessName
                : sys.ProcessName.Trim();
            healthChecks.AddProcessHealthCheck(
                processName,
                static procs => procs.Length > 0,
                name: "system-process",
                failureStatus: HealthStatus.Unhealthy,
                tags: livenessTags,
                timeout: timeout);
        }

        if (OperatingSystem.IsWindows())
        {
            if (sys.WindowsServiceName is not null && string.IsNullOrWhiteSpace(sys.WindowsServiceName))
            {
                // disabled explicitly
            }
            else
            {
                var serviceName = string.IsNullOrWhiteSpace(sys.WindowsServiceName)
                    ? "RpcSs"
                    : sys.WindowsServiceName.Trim();
                var machine = string.IsNullOrWhiteSpace(sys.WindowsServiceMachineName) ? "." : sys.WindowsServiceMachineName.Trim();
#pragma warning disable CA1416 // ServiceController: Windows-only; branch guarded by IsWindows()
                healthChecks.AddWindowsServiceHealthCheck(
                    serviceName,
                    static sc => sc.Status == ServiceControllerStatus.Running,
                    machine,
                    name: "system-windows-service",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: livenessTags,
                    timeout: timeout);
#pragma warning restore CA1416
            }
        }
    }
}
