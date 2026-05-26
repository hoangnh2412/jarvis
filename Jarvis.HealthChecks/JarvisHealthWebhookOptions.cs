// Jarvis.HealthChecks — HealthChecks UI webhook notification model (failure + restored payloads).
namespace Jarvis.HealthChecks;

/// <summary>
/// Webhook notification definition consumed by HealthChecks UI when status changes.
/// Định nghĩa webhook dùng bởi HealthChecks UI khi trạng thái thay đổi.
/// </summary>
public sealed class JarvisHealthWebhookOptions
{
    /// <summary>
    /// Logical name for this webhook in UI configuration.
    /// Tên logic của webhook trong cấu hình UI.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Target webhook URL (Slack incoming webhook, Teams connector, etc.).
    /// URL webhook đích (Slack, Teams connector, v.v.).
    /// </summary>
    public string Uri { get; set; } = "";

    /// <summary>
    /// JSON body template sent when a health check becomes unhealthy.
    /// Mẫu JSON gửi khi health check chuyển sang unhealthy.
    /// </summary>
    public string Payload { get; set; } = "";

    /// <summary>
    /// JSON body template sent when health recovers to healthy.
    /// Mẫu JSON gửi khi health phục hồi về healthy.
    /// </summary>
    public string RestoredPayload { get; set; } = "";
}
