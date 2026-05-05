namespace Jarvis.HealthChecks;

/// <summary>
/// Options for AspNetCore.HealthChecks.UI; history uses InMemory EF Core database only.
/// Tùy chọn cho AspNetCore.HealthChecks.UI; lịch sử chỉ dùng cơ sở dữ liệu InMemory (EF Core).
/// </summary>
public sealed class JarvisHealthCheckUiOptions
{
    /// <summary>
    /// When true, registers UI services and maps <c>MapHealthChecksUI</c> from calling host.
    /// Khi true, đăng ký dịch vụ UI và map <c>MapHealthChecksUI</c> từ host gọi.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Interval in seconds between UI polling rounds against configured endpoints.
    /// Khoảng thời gian (giây) giữa các vòng poll của UI tới các endpoint đã cấu hình.
    /// </summary>
    public int EvaluationTimeSeconds { get; set; } = 10;

    /// <summary>
    /// Browser path segment for the SPA dashboard (no trailing slash stored by mapper).
    /// Phân đoạn đường dẫn trình duyệt cho dashboard SPA (mapper xử lý không có slash cuối).
    /// </summary>
    public string UIPath { get; set; } = "/healthchecks-ui";

    /// <summary>
    /// Path segment served by the UI JSON API middleware.
    /// Phân đoạn đường dẫn cho middleware API JSON của UI.
    /// </summary>
    public string ApiPath { get; set; } = "/healthchecks-api";

    /// <summary>
    /// Named endpoints the UI worker HTTP-clients poll (must be absolute URIs).
    /// Các endpoint có tên mà worker của UI gọi HTTP (phải là URI tuyệt đối).
    /// </summary>
    public List<JarvisHealthUiEndpointOptions> Endpoints { get; set; } = [];

    /// <summary>
    /// Optional Slack/Teams-style webhook payloads for failure/recovery notifications.
    /// Webhook tùy chọn (Slack/Teams) để thông báo khi lỗi hoặc phục hồi.
    /// </summary>
    public List<JarvisHealthWebhookOptions> Webhooks { get; set; } = [];
}
