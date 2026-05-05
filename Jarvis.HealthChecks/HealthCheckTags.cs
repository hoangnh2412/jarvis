namespace Jarvis.HealthChecks;

/// <summary>
/// Constant tag strings used to filter health checks per Kubernetes probe type and dashboards.
/// Chuỗi tag cố định để lọc health check theo loại probe Kubernetes và dashboard.
/// </summary>
public static class HealthCheckTags
{
    /// <summary>Liveness probe tag / Tag cho probe liveness (tiến trình còn sống).</summary>
    public const string Liveness = "liveness";

    /// <summary>Readiness probe tag / Tag cho probe readiness (sẵn sàng nhận traffic).</summary>
    public const string Readiness = "readiness";

    /// <summary>Startup probe tag / Tag cho probe startup (khởi động xong).</summary>
    public const string Startup = "startup";

    /// <summary>Detailed report grouping / Nhóm báo cáo chi tiết (endpoint đầy đủ).</summary>
    public const string Detailed = "detailed";

    /// <summary>Integration metrics grouping / Nhóm metric tích hợp (queue, v.v.).</summary>
    public const string Integration = "integration";
}
