namespace Sample.Health;

/// <summary>
/// Disk space readiness rule for a single mount or drive letter (Sample <c>ReadinessHealthChecks:DiskPaths</c>).
/// Quy tắc readiness dung lượng ổ đĩa cho một điển mount hoặc ký tự ổ.
/// </summary>
public sealed class SampleReadinessDiskPathOptions
{
    /// <summary>
    /// Path to monitor (e.g. <c>/</c> on Linux or <c>C:\</c> on Windows).
    /// Đường dẫn cần giám sát (vd. <c>/</c> trên Linux hoặc <c>C:\</c> trên Windows).
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Minimum free space required in megabytes before reporting failure status.
    /// Dung lượng trống tối thiểu (MB) trước khi báo trạng thái lỗi.
    /// </summary>
    public long MinimumFreeMegabytes { get; set; } = 256;
}
