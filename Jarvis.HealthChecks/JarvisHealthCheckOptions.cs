// Jarvis.HealthChecks — Root options for section "HealthChecks": liveness thresholds, Prometheus, detailed /health gate, UI, System package probes.
namespace Jarvis.HealthChecks;

/// <summary>
/// Strongly typed configuration bound from the <c>HealthChecks</c> configuration section (liveness + hosting endpoints).
/// Cấu hình kiểu mạnh bind từ section <c>HealthChecks</c> (liveness + endpoint host).
/// </summary>
public sealed class JarvisHealthCheckOptions
{
    /// <summary>Configuration section key / Khóa section cấu hình.</summary>
    public const string SectionName = "HealthChecks";

    /// <summary>
    /// <c>process-resources</c> liveness: fail when <c>(CLR allocated MB / <see cref="ProcessAllocatedMemoryMegabytesCeiling"/>) * 100</c>
    /// is at or above this value (default 80). Uses <c>GC.GetTotalMemory(false)</c>, aligned with <c>process-allocated-memory</c>.
    /// Set <see cref="ProcessAllocatedMemoryMegabytesCeiling"/> to 0 to skip this memory ratio (always healthy for memory).
    /// Liveness <c>process-resources</c>: thất bại khi <c>(MB CLR ước lượng / <see cref="ProcessAllocatedMemoryMegabytesCeiling"/>) * 100</c>
    /// đạt hoặc vượt giá trị này (mặc định 80). Dùng <c>GC.GetTotalMemory(false)</c>, đồng bộ với <c>process-allocated-memory</c>.
    /// Đặt <see cref="ProcessAllocatedMemoryMegabytesCeiling"/> = 0 để bỏ qua tỷ lệ RAM này (memory luôn healthy).
    /// </summary>
    public int MemoryThresholdPercent { get; set; } = 80;

    /// <summary>
    /// <c>process-resources</c> liveness: CPU ceiling in percent (0–100) for the app’s normalized usage; fail when measured CPU ≥ this.
    /// Displayed <c>cpu_load</c> is <c>(measured / this) * 100</c> (100% = at ceiling). Default 90. Use 0 to skip CPU check.
    /// Liveness <c>process-resources</c>: trần %CPU (0–100) cho usage tiến trình; thất bại khi CPU đo được ≥ giá trị này.
    /// <c>cpu_load</c> hiển thị <c>(đo được / trần) * 100</c> (100% = chạm trần). Mặc định 90. Đặt 0 để bỏ qua kiểm CPU.
    /// </summary>
    public int CpuThresholdPercent { get; set; } = 90;

    /// <summary>
    /// Short TTL cache for liveness results to keep responses fast under concurrent scraping.
    /// TTL cache ngắn cho kết quả liveness để phản hồi nhanh khi nhiều request đồng thời.
    /// </summary>
    public int ResultCacheMilliseconds { get; set; } = 100;

    /// <summary>
    /// CLR allocated memory ceiling in MB for <c>process-allocated-memory</c> and for <c>process-resources</c> <c>memory_load</c> denominator (0 disables the separate allocated check only; <c>process-resources</c> memory ratio is skipped when 0).
    /// Trần MB bộ nhớ CLR cho <c>process-allocated-memory</c> và mẫu số <c>memory_load</c> của <c>process-resources</c> (0 chỉ tắt check allocated riêng; tỷ lệ memory của <c>process-resources</c> bị bỏ qua khi 0).
    /// </summary>
    public int ProcessAllocatedMemoryMegabytesCeiling { get; set; } = 4096;

    /// <summary>
    /// When set, the detailed <c>/health</c> JSON requires this secret header value (see <see cref="DetailedEndpointApiKeyHeader"/>).
    /// Public probes remain unauthenticated.
    /// Khi có giá trị, JSON chi tiết <c>/health</c> yêu cầu header bí mật (xem <see cref="DetailedEndpointApiKeyHeader"/>).
    /// Các probe công khai vẫn không xác thực.
    /// </summary>
    public string? DetailedEndpointApiKey { get; set; }

    /// <summary>
    /// HTTP header name carrying <see cref="DetailedEndpointApiKey"/>.
    /// Tên header HTTP chứa <see cref="DetailedEndpointApiKey"/>.
    /// </summary>
    public string DetailedEndpointApiKeyHeader { get; set; } = "X-Health-Detailed-Key";

    /// <summary>
    /// Enables Prometheus exporter middleware path from options.
    /// Bật middleware exporter Prometheus theo đường dẫn trong cấu hình.
    /// </summary>
    public bool EnablePrometheusMetrics { get; set; } = true;

    /// <summary>
    /// Relative URL path for Prometheus scrape endpoint (OTLP may still export separately).
    /// Đường dẫn URL tương đối cho endpoint scrape Prometheus (OTLP có thể export riêng).
    /// </summary>
    public string PrometheusMetricsPath { get; set; } = "/health/prometheus";

    /// <summary>
    /// Conventional default probe timeout in seconds (1–120 typical). Core does not register readiness checks, so the host
    /// should read this property (or bind <c>HealthChecks:DefaultTimeoutSeconds</c>) when adding SQL/Redis/HTTP readiness.
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// When <c>true</c> (default), <c>UseHealthChecks</c> subscribes to <c>IHostApplicationLifetime.ApplicationStarted</c> and calls
    /// <see cref="IStartupCompletionNotifier.MarkStartupComplete"/> so <c>/health/startup</c> succeeds without boilerplate in <c>Program.cs</c>.
    /// Set <c>false</c> if startup must remain unhealthy until custom work finishes after the host starts (e.g. async init in a hosted service).
    /// Synchronous work before <c>Run()</c> (migrations, etc.) still completes before <c>ApplicationStarted</c> fires.
    /// </summary>
    public bool MarkStartupCompleteOnApplicationStarted { get; set; } = true;

    /// <summary>
    /// HealthChecks UI settings (InMemory storage).
    /// Cấu hình HealthChecks UI (lưu trữ InMemory).
    /// </summary>
    public JarvisHealthCheckUiOptions Ui { get; set; } = new();

    /// <summary>
    /// Optional AspNetCore.HealthChecks.System liveness checks under configuration section <c>HealthChecks:System</c>.
    /// </summary>
    public JarvisHealthCheckSystemOptions System { get; set; } = new();
}
