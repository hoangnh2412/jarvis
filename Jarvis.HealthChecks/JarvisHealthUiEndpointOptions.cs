namespace Jarvis.HealthChecks;

/// <summary>
/// One HealthChecks UI monitored endpoint (display name + poll URI).
/// Một endpoint được UI giám sát (tên hiển thị + URI để poll).
/// </summary>
public sealed class JarvisHealthUiEndpointOptions
{
    /// <summary>
    /// Friendly label shown in the UI grid.
    /// Nhãn thân thiện hiển thị trong lưới UI.
    /// </summary>
    public string Name { get; set; } = "API";

    /// <summary>
    /// Absolute URI returning health JSON (often <c>/health</c> on the same app).
    /// URI tuyệt đối trả JSON health (thường là <c>/health</c> trên cùng app).
    /// </summary>
    public string Uri { get; set; } = "";
}
