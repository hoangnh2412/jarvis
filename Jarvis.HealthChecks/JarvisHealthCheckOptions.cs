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
    /// Liveness fails when GC-reported memory load is at or above this percentage (default 80).
    /// Liveness thất bại khi mức bộ nhớ theo GC đạt hoặc vượt ngưỡng phần trăm này (mặc định 80).
    /// </summary>
    public int MemoryThresholdPercent { get; set; } = 80;

    /// <summary>
    /// Liveness fails when estimated process CPU usage is at or above this percentage (default 90).
    /// Liveness thất bại khi CPU tiến trình ước lượng đạt hoặc vượt ngưỡng phần trăm này (mặc định 90).
    /// </summary>
    public int CpuThresholdPercent { get; set; } = 90;

    /// <summary>
    /// Short TTL cache for liveness results to keep responses fast under concurrent scraping.
    /// TTL cache ngắn cho kết quả liveness để phản hồi nhanh khi nhiều request đồng thời.
    /// </summary>
    public int ResultCacheMilliseconds { get; set; } = 100;

    /// <summary>
    /// AspNetCore.HealthChecks.System: fail liveness when CLR allocated memory exceeds this MB (0 disables).
    /// AspNetCore.HealthChecks.System: liveness thất bại khi bộ nhớ CLR vượt ngưỡng MB này (0 = tắt).
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
    /// HealthChecks UI settings (InMemory storage).
    /// Cấu hình HealthChecks UI (lưu trữ InMemory).
    /// </summary>
    public JarvisHealthCheckUiOptions Ui { get; set; } = new();
}
