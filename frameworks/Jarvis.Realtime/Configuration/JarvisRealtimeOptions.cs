namespace Jarvis.Realtime.Configuration;

/// <summary>
/// Tùy chọn transport Realtime / SignalR, bind từ <c>Realtime:SignalR</c>.
/// </summary>
public sealed class JarvisRealtimeOptions
{
    public const string SectionName = "Realtime:SignalR";

    public string HubPath { get; set; } = "/hubs/notifications";

    /// <summary>Tên method client nhận message realtime (mặc định <c>notification</c>).</summary>
    public string ClientMethodName { get; set; } = "notification";

    public bool UseRedisBackplane { get; set; }

    public RealtimeSignalRRedisOptions Redis { get; set; } = new();
}
