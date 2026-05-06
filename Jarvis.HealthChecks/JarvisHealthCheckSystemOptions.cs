// Jarvis.HealthChecks — Sub-options under HealthChecks:System for optional AspNetCore.HealthChecks.System liveness checks.
namespace Jarvis.HealthChecks;

/// <summary>
/// AspNetCore.HealthChecks.System liveness checks. Non-zero limits and non-empty lists register checks; bind <c>HealthChecks:System</c> to override.
/// Use empty string for <see cref="ProcessName"/> or <see cref="WindowsServiceName"/> to disable that check.
/// </summary>
public sealed class JarvisHealthCheckSystemOptions
{
    /// <summary>Max private bytes = value × 1 MiB; 0 = do not register <c>AddPrivateMemoryHealthCheck</c>.</summary>
    public int PrivateMemoryMegabytesMaximum { get; set; } = 8192;

    /// <summary>Max working set bytes = value × 1 MiB; 0 = do not register <c>AddWorkingSetHealthCheck</c>.</summary>
    public int WorkingSetMegabytesMaximum { get; set; } = 8192;

    /// <summary>Max virtual memory size bytes = value × 1 MiB; 0 = do not register <c>AddVirtualMemorySizeHealthCheck</c>.</summary>
    public int VirtualMemorySizeMegabytesMaximum { get; set; } = 1_048_576;

    /// <summary>Drives for <c>AddDiskStorageHealthCheck</c> (empty = skip).</summary>
    public List<JarvisHealthCheckSystemDiskPathOptions> DiskDrives { get; set; } =
    [
        new() { Path = "/", MinimumFreeMegabytes = 256 },
    ];

    /// <summary>Folder paths for <c>AddFolder</c> (empty = skip).</summary>
    public List<string> MonitorFolders { get; set; } = ["."];

    /// <summary>When true, every monitored folder must pass (library option <c>CheckAllFolders</c>).</summary>
    public bool FolderCheckAll { get; set; }

    /// <summary>File paths for <c>AddFile</c> (empty = skip).</summary>
    public List<string> MonitorFiles { get; set; } = ["appsettings.json"];

    /// <summary>When true, every monitored file must pass (<c>CheckAllFiles</c>).</summary>
    public bool FileCheckAll { get; set; }

    /// <summary>
    /// Process name without extension for <c>AddProcessHealthCheck</c>. <c>null</c> = current process; <c>""</c> = do not register.
    /// </summary>
    public string? ProcessName { get; set; }

    /// <summary>
    /// Windows only: service short name for <c>AddWindowsServiceHealthCheck</c>. <c>null</c> = <c>RpcSs</c>; <c>""</c> = do not register.
    /// </summary>
    public string? WindowsServiceName { get; set; }

    /// <summary>Windows only: machine name for <c>AddWindowsServiceHealthCheck</c> (default <c>.</c> local).</summary>
    public string WindowsServiceMachineName { get; set; } = ".";
}

/// <summary>One drive or mount path entry for <see cref="JarvisHealthCheckSystemOptions.DiskDrives"/>.</summary>
public sealed class JarvisHealthCheckSystemDiskPathOptions
{
    /// <summary>Drive root or folder path (e.g. <c>/</c> on Linux, <c>C:\</c> on Windows).</summary>
    public string Path { get; set; } = "";

    /// <summary>Minimum free space in megabytes (passed to <c>AddDrive</c> as in host samples).</summary>
    public long MinimumFreeMegabytes { get; set; } = 256;
}
