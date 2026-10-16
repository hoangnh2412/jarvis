namespace Platform.Realtime.Configuration;

public sealed class RealtimeSseOptions
{
    public string SsePath { get; set; } = "/sse/notifications";
    public bool Enabled { get; set; } = true;
    public int HeartbeatSeconds { get; set; } = 15;
    public bool UseRedisFanOut { get; set; } = false;
    public bool AllowAccessTokenQuery { get; set; } = false;
    public string ChannelPrefix { get; set; } = "platform-realtime-sse";
}