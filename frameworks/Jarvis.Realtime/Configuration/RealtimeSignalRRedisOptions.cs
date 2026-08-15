namespace Jarvis.Realtime.Configuration;

/// <summary>
/// Tùy chọn Redis backplane cho SignalR scale-out.
/// </summary>
public sealed class RealtimeSignalRRedisOptions
{
    public string Configuration { get; set; } = string.Empty;

    /// <summary>Prefix kênh Redis backplane — bind từ <c>Realtime:SignalR:Redis:ChannelPrefix</c>.</summary>
    public string ChannelPrefix { get; set; } = "jarvis-realtime";
}
