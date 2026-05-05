namespace Jarvis.HealthChecks;

/// <summary>
/// Optional extension point for domain metrics text appended to readiness/detailed health descriptions.
/// Điểm mở rộng tùy chọn để gắn chuỗi metric nghiệp vụ vào mô tả readiness/chi tiết.
/// </summary>
public interface IJarvisHealthIntegrationMetricsProvider
{
    /// <summary>
    /// Builds a short human-readable metrics fragment for <c>HealthCheckResult.Description</c>.
    /// Tạo đoạn metric ngắn, dễ đọc cho <c>HealthCheckResult.Description</c>.
    /// </summary>
    /// <param name="cancellationToken">EN: Cooperative cancellation / VI: Hủy hợp tác.</param>
    Task<string> GetMetricsDescriptionAsync(CancellationToken cancellationToken = default);
}
