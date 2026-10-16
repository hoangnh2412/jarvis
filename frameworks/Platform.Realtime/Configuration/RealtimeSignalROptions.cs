namespace Platform.Realtime.Configuration;

/// <summary>
/// Tùy chọn transport Realtime / SignalR, bind từ <c>Realtime:SignalR</c>.
/// </summary>
public sealed class RealtimeSignalROptions
{
    public string SignalRPath { get; set; } = "/hubs/notifications";
    public bool Enabled { get; set; } = true;
    public bool UseRedisBackplane { get; set; }
    public string ChannelPrefix { get; set; } = "platform-realtime-signalr";
}
